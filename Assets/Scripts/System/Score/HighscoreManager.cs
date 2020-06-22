using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using DBreeze;
using DBreeze.DataTypes;
using DBreeze.Utils;

public static class HighscoreManager {

	// TODO: player name, separate top ten for each player name

	public class RemixEntry {
		public long EntryId { get; set; } = 0;
		public string RemixId { get; set; }
	}

	public class HighscoreEntry {

		public long EntryId { get; set; } = 0;
		public long RemixEntryId { get; set; } // not needed?
		public long PlayerEntryId { get; set; } = 0;

		public DateTime udtCreated { get; set; }

		public int Score { get; set; }
		public int Time { get; set; }
		public CharacterSelected Character { get; set; }
		// public string RemixID;

		public HighscoreEntry() {
			udtCreated = DateTime.UtcNow;
		}
	}

	public class PlayerEntry {
		public long EntryId { get; set; } = 0;
		public string Name { get; set; }
	}

	// [Serializable]
	public class HighscoreList {

		public static string dbPath = Application.dataPath + "/DBR1";

		public DBreezeEngine engine = null;

		// IDEA: only save top 10 in each category

		public HighscoreList(bool initDb = true) {

			if (initDb) {
				InitDB();
			}
		}

		~HighscoreList() {
			engine?.Dispose();
			Debug.Log("Disposed db");
		}

		public void InitDB() {
			if (engine == null) {
				// engine = ​new DBreezeEngine(​@"D:\temp\DBR1"​);
				engine = new DBreezeEngine(dbPath);
				Debug.Log("loaded db using " + dbPath);
			}

			//Setting up NetJSON serializer (from NuGet) to be used by DBreeze
			// DBreeze.Utils.CustomSerializator.ByteArraySerializator = (object o) => { return NetJSON.NetJSON.Serialize(o).To_UTF8Bytes(); };
			// DBreeze.Utils.CustomSerializator.ByteArrayDeSerializator = (byte[] bt, Type t) => { return NetJSON.NetJSON.Deserialize(t, bt.UTF8_GetString()); };

		}

		// public void Dispose(){
		// 	engine?.Dispose();
		// }


		// public static List<HighscoreEntry> Entries;
		public Dictionary<string, List<HighscoreEntry>> Entries = new Dictionary<string, List<HighscoreEntry>>();

		// public static HighscoreList LoadListFromFile(string path) {
		// 	return JsonUtility.FromJson<HighscoreList>(File.ReadAllText(path));
		// }

		public void LoadListFromServer(string url) {

		}

		// public void SaveListToFile(string path) {
		// 	File.WriteAllText(dbPath, JsonUtility.ToJson(this, true));
		// }

		public void SaveListToServer(string url) {

		}

		// Sort one list
		// NOTE: truncates list
		public void Sort(string remixId, bool skipKeyCheck = false) {
			if (!skipKeyCheck && !Entries.ContainsKey(remixId))
				return;

			Entries[remixId] = Entries[remixId].OrderByDescending(x => x.Score).Take(10).ToList();
		}

		// sort all lists
		public void Sort() {
			foreach (var item in Entries) {
				Sort(item.Key, true);
			}
		}

		public void AddScore(string remixId, HighscoreEntry entry) {
			if (!Entries.ContainsKey(remixId))
				Entries.Add(remixId, new List<HighscoreEntry>());

			Entries[remixId].Add(entry);
			// TODO: sort and truncate
		}

		// TODO: search all entries for top 10 highest scores
		// IDEA: keep cached list, marked dirty on adding score to any list, update on get if dirty
		// public IEnumerable<(string, HighscoreEntry)> GetAllTimeHighscore() { }

		public void Start() {
			//Inserting CustomerId 1
			var remix = new RemixEntry() { RemixId = "Tino Zanner" };
			Test_InsertRemix(remix);

			//Inserting some orders for this customer
			Test_InsertHighscore(
				Enumerable.Range(1, 5)
				.Select(r => new HighscoreEntry { RemixEntryId = remix.EntryId })
				);

			//Test update order 
			Test_UpdateHighscore(3);

			//Inserting CustomerId 2
			remix = new RemixEntry() { RemixId = "Michael Hinze" };
			Test_InsertRemix(remix);

			//Inserting some orders for this customer
			Test_InsertHighscore(
				Enumerable.Range(1, 8)
				.Select(r => new HighscoreEntry { RemixEntryId = remix.EntryId })
				);


			//-------------------------- Various data retrieving

			//Getting Customer ById
			Test_GetRemixByEntryId(2);

			//Getting Customer ByName
			Test_GetRemixByFreeText("ichael");

			//Getting all orders
			Console.WriteLine("All orders");
			Test_GetHigscoresByDateTimeRange(DateTime.MinValue, DateTime.MaxValue);

			//Getting Orders of customer 1
			Console.WriteLine("Orders of customer 1");
			Test_GetHighscoresByRemixEntryIdAndDateTimeRange(1, DateTime.MinValue, DateTime.MaxValue);

			////Getting Orders of customer 2
			Console.WriteLine("Orders of customer 2");
			Test_GetHighscoresByRemixEntryIdAndDateTimeRange(2, DateTime.MinValue, DateTime.MaxValue);

		}



		const string playerTable = "Players";
		void Test_InsertPlayer(PlayerEntry entry) {
			try {
				/*
                    We are going to store all customers in one table
                    Later we are going to search customers by their IDs and Names
                */

				using (var t = engine.GetTransaction()) {
					//Documentation https://goo.gl/Kwm9aq
					//This line with a list of tables we need in case if we modify more than 1 table inside of transaction
					t.SynchronizeTables(playerTable);

					// bool newEntity = entry.Id== 0;
					bool newEntity = entry.EntryId == 0;
					if (newEntity)
						entry.EntryId = t.ObjectGetNewIdentity<long>(playerTable);

					//Documentation https://goo.gl/YtWnAJ
					t.ObjectInsert(playerTable, new DBreeze.Objects.DBreezeObject<PlayerEntry> {
						// NewEntity = newEntity,
						NewEntity = false,
						Entity = entry,
						Indexes = new List<DBreeze.Objects.DBreezeIndex> {
							//to Get customer by ID
							// new DBreeze.Objects.DBreezeIndex(1,entry.Id) { PrimaryIndex = true },
							// new DBreeze.Objects.DBreezeIndex(1,entry.) { PrimaryIndex = true },
						}
					}, false);

					//Documentation https://goo.gl/s8vtRG
					//Setting text search index. We will store text-search 
					//indexes concerning customers in table "TS_Customers".
					//Second parameter is a reference to the customer ID.
					// t.TextInsert("TS_Customers", entry.Id.ToBytes(), entry.Name);

					//Committing entry
					t.Commit();
				}

			} catch (Exception ex) {
				throw ex;
			}

		}


		const string remixTable = "Remixes";
		void Test_InsertRemix(RemixEntry entry) {

			try {
				/*
                    We are going to store all customers in one table
                    Later we are going to search customers by their IDs and Names
                */

				using (var t = engine.GetTransaction()) {
					//Documentation https://goo.gl/Kwm9aq
					//This line with a list of tables we need in case if we modify more than 1 table inside of transaction
					t.SynchronizeTables(remixTable);

					// bool newEntity = entry.Id== 0;
					bool newEntity = entry.EntryId == 0;
					if (newEntity)
						entry.EntryId = t.ObjectGetNewIdentity<long>(remixTable);

					//Documentation https://goo.gl/YtWnAJ
					t.ObjectInsert(remixTable, new DBreeze.Objects.DBreezeObject<RemixEntry> {
						// NewEntity = newEntity,
						NewEntity = false,
						Entity = entry,
						Indexes = new List<DBreeze.Objects.DBreezeIndex> {
							//to Get customer by ID
							// new DBreeze.Objects.DBreezeIndex(1,entry.Id) { PrimaryIndex = true },
							// new DBreeze.Objects.DBreezeIndex(1,entry.) { PrimaryIndex = true },
						}
					}, false);

					//Documentation https://goo.gl/s8vtRG
					//Setting text search index. We will store text-search 
					//indexes concerning customers in table "TS_Customers".
					//Second parameter is a reference to the customer ID.
					// t.TextInsert("TS_Customers", entry.Id.ToBytes(), entry.Name);

					//Committing entry
					t.Commit();
				}

			} catch (Exception ex) {
				throw ex;
			}

		}


		const string highscoreTable = "Highscores";
		void Test_InsertHighscore(IEnumerable<HighscoreEntry> highscores) {
			try {
				/*
                We are going to store all orders from all customers in one table.
                Later we are planning to search orders:
                    1. by Order.Id
                    2. by Order.udtCreated From-To
                    3. by Order.CustomerId and Order.udtCreated From-To
                */

				using (var t = engine.GetTransaction()) {
					//This line with a list of tables we need in case if we modify morethen 1 table inside of transaction
					//Documentation https://goo.gl/Kwm9aq
					t.SynchronizeTables(highscoreTable);

					foreach (var highscore in highscores) {
						// bool newEntity = order.Id == 0;
						bool newEntity = highscore.EntryId == 0;
						if (newEntity)
							highscore.EntryId = t.ObjectGetNewIdentity<long>(highscoreTable);

						t.ObjectInsert(highscoreTable, new DBreeze.Objects.DBreezeObject<HighscoreEntry> {
							NewEntity = newEntity,
							Indexes = new List<DBreeze.Objects.DBreezeIndex>
							 {
                                 //to Get order by ID
                                 new DBreeze.Objects.DBreezeIndex(1,highscore.EntryId) { PrimaryIndex = true },
                                 //to get orders in specified time interval
                                 new DBreeze.Objects.DBreezeIndex(2,highscore.udtCreated) { AddPrimaryToTheEnd = true }, //AddPrimaryToTheEnd by default is true
                                 //to get orders in specified time range for specific customer
                                 new DBreeze.Objects.DBreezeIndex(3,highscore.RemixEntryId, highscore.udtCreated)
							 },
							Entity = highscore  //Setting entity
						}, false);  //set last parameter to true, if batch operation speed unsatisfactory
					}

					//Committing all changes
					t.Commit();

				}
			} catch (Exception ex) {
				throw ex;
			}
		}


		void Test_UpdateHighscore(long orderId) {
			try {

				using (var t = engine.GetTransaction()) {
					//This line with a list of tables we need in case if we modify morethen 1 table inside of transaction
					//Documentation https://goo.gl/Kwm9aq
					t.SynchronizeTables(highscoreTable);

					var ord = t.Select<byte[], byte[]>(highscoreTable, 1.ToIndex(orderId)).ObjectGet<HighscoreEntry>();
					if (ord == null)
						return;

					ord.Entity.udtCreated = new DateTime(1977, 1, 1);
					ord.Indexes = new List<DBreeze.Objects.DBreezeIndex>() {
						//to Get order by ID
						new DBreeze.Objects.DBreezeIndex(1,ord.Entity.EntryId) { PrimaryIndex = true },
						//to get orders in specified time interval
						new DBreeze.Objects.DBreezeIndex(2,ord.Entity.udtCreated), //AddPrimaryToTheEnd by default is true
						//to get orders in specified time range for specific customer
						new DBreeze.Objects.DBreezeIndex(3,ord.Entity.RemixEntryId, ord.Entity.udtCreated)
					};

					t.ObjectInsert<HighscoreEntry>(highscoreTable, ord, false);

					//Committing all changes
					t.Commit();
				}
			} catch (Exception ex) {
				throw ex;
			}
		}

		void Test_GetHigscoresByDateTimeRange(DateTime from, DateTime to) {
			try {
				using (var t = engine.GetTransaction()) {
					//Documentation https://goo.gl/MbZAsB
					foreach (var row in t.SelectForwardFromTo<byte[], byte[]>(highscoreTable,
						2.ToIndex(from, long.MinValue), true,
						2.ToIndex(to, long.MaxValue), true)) {
						var obj = row.ObjectGet<HighscoreEntry>();
						if (obj != null)
							Console.WriteLine(
								obj.Entity.EntryId
								+ " " + obj.Entity.udtCreated.ToString("dd.MM.yyyy HH:mm:ss.fff")
								+ " " + obj.Entity.RemixEntryId);
					}
				}
			} catch (Exception ex) {
				throw ex;
			}
		}


		void Test_GetHighscoresByRemixEntryIdAndDateTimeRange(long remixEntryId, DateTime from, DateTime to) {
			try {
				using (var t = engine.GetTransaction()) {
					foreach (var row in t.SelectForwardFromTo<byte[], byte[]>(highscoreTable,
						3.ToIndex(remixEntryId, from, long.MinValue), true,
						3.ToIndex(remixEntryId, to, long.MaxValue), true)) {
						var obj = row.ObjectGet<HighscoreEntry>();
						if (obj != null)
							Console.WriteLine(
								obj.Entity.EntryId
								+ " " + obj.Entity.udtCreated.ToString("dd.MM.yyyy HH:mm:ss.fff")
								+ " " + obj.Entity.RemixEntryId);
					}
				}
			} catch (Exception ex) {
				throw ex;
			}
		}


		void Test_GetRemixByEntryId(long remixEntryId) {
			try {
				using (var t = engine.GetTransaction()) {
					var obj = t.Select<byte[], byte[]>(remixTable, 1.ToIndex(remixEntryId)).ObjectGet<RemixEntry>();
					if (obj != null)
						Console.WriteLine(obj.Entity.EntryId + " " + obj.Entity.RemixId);
				}
			} catch (Exception ex) {
				throw ex;
			}
		}

		void Test_GetRemixByFreeText(string text) {
			try {
				using (var t = engine.GetTransaction()) {
					// foreach (var doc in t.TextSearch("TS_Customers").BlockAnd(text).GetDocumentIDs()) {
					foreach (var doc in t.TextSearch("TS_Remixes").BlockAnd(text).GetDocumentIDs()) {
						var obj = t.Select<byte[], byte[]>(remixTable, 1.ToIndex(doc)).ObjectGet<RemixEntry>();
						if (obj != null)
							Console.WriteLine(obj.Entity.EntryId + " " + obj.Entity.RemixId);
					}
				}
			} catch (Exception ex) {
				throw ex;
			}
		}

	}



	public static HighscoreList List = new HighscoreList();



	// public static string SavePath = Application.dataPath + "/highscore.json";
	// public static string dbPath = Application.dataPath + "/DBR1";

	// TODO: async/coroutine fn for getting entries

	// public static void Init() {

	// }

}
