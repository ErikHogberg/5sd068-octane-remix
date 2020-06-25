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

	// [Serializable]
	public class RemixEntry {
		public long EntryId { get; set; } = 0;
		public string RemixId { get; set; }
	}

	public class HighscoreEntry {

		public long EntryId { get; set; } = 0;
		public long RemixEntryId { get; set; }
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

	public class HighscoreList {

		public static string dbPath = Application.dataPath + "/HighscoreDatabase";//"/DBR1";

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

			// DBreeze.Utils.CustomSerializator.ByteArraySerializator = (object o) => { return JsonUtility.ToJson(o).To_UTF8Bytes(); };
			// DBreeze.Utils.CustomSerializator.ByteArrayDeSerializator = (byte[] bt, Type t) => { return JsonUtility.FromJson(bt.UTF8_GetString(), t); };

			DBreeze.Utils.CustomSerializator.ByteArraySerializator = (object o) => { return Newtonsoft.Json.JsonConvert.SerializeObject(o).To_UTF8Bytes(); };
			DBreeze.Utils.CustomSerializator.ByteArrayDeSerializator = (byte[] bt, Type t) => { return Newtonsoft.Json.JsonConvert.DeserializeObject(bt.UTF8_GetString(), t); };

		}

		// public void Dispose(){
		// 	engine?.Dispose();
		// }

		// public Dictionary<string, List<HighscoreEntry>> Entries = new Dictionary<string, List<HighscoreEntry>>();

		/*
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

		*/

		public void Start(bool insert = true) {

			if (insert) {
				Debug.Log("insert remix");

				//* Inserting CustomerId 1
				var remix = new RemixEntry() { RemixId = "asdf" };
				Insert(remix);

				Debug.Log("insert player");
				var player = new PlayerEntry() { Name = "Tino Zanner" };
				Insert(player);

				Debug.Log("insert highscores");
				//* Inserting some orders for this customer
				Insert(
					Enumerable.Range(1, 5)
					.Select(r => new HighscoreEntry { RemixEntryId = remix.EntryId, PlayerEntryId = player.EntryId, Score = 100, Time = 200, Character = CharacterSelected.NONE })
					);

				Debug.Log("update highscore");
				//* Test update order 
				UpdateHighscore(3);

				Debug.Log("insert remix 2");
				//* Inserting CustomerId 2
				remix = new RemixEntry() { RemixId = "Michael Hinze" };
				Insert(remix);

				Debug.Log("insert highscores 2");
				//* Inserting some orders for this customer
				Insert(
					Enumerable.Range(1, 8)
					.Select(r => new HighscoreEntry { RemixEntryId = remix.EntryId, PlayerEntryId = player.EntryId, Score = 100, Time = 200, Character = CharacterSelected.AKASH })
				);
			}

			Debug.Log("get remix by entry id");
			//* Getting Customer ById
			GetRemixByEntryId(2);

			Debug.Log("get remix by remix id text");
			//* Getting Customer ByName
			GetRemixByFreeText("ichael");

			Debug.Log("get highscores by remix");
			GetHighscoresByRemix(1);
			Debug.Log("get highscores by remix and player");
			GetHighscoresByRemixAndPlayer(1, 1);

			//* Getting all orders
			// Console.WriteLine("All orders");
			// Test_GetHigscoresByDateTimeRange(DateTime.MinValue, DateTime.MaxValue);

			//* Getting Orders of customer 1
			// Console.WriteLine("Orders of customer 1");
			// Test_GetHighscoresByRemixEntryIdAndDateTimeRange(1, DateTime.MinValue, DateTime.MaxValue);

			//* Getting Orders of customer 2
			// Console.WriteLine("Orders of customer 2");
			// Test_GetHighscoresByRemixEntryIdAndDateTimeRange(2, DateTime.MinValue, DateTime.MaxValue);


		}


		const string playerTable = "Players";
		const string playerTsTable = "TS_Players";

		void Insert(PlayerEntry entry) {
			try {
				//* We are going to store all customers in one table
				//* Later we are going to search customers by their IDs and Names

				using (var t = engine.GetTransaction()) {
					//Documentation https://goo.gl/Kwm9aq
					//* This line with a list of tables we need in case if we modify more than 1 table inside of transaction
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
							//*to Get customer by ID
							// new DBreeze.Objects.DBreezeIndex(1,entry.Id) { PrimaryIndex = true },
							new DBreeze.Objects.DBreezeIndex(1, entry.EntryId) { PrimaryIndex = true },
						}
					}, false);

					//Documentation https://goo.gl/s8vtRG
					//* Setting text search index. We will store text-search 
					//* indexes concerning customers in table "TS_Customers".
					//* Second parameter is a reference to the customer ID.
					// t.TextInsert("TS_Customers", entry.Id.ToBytes(), entry.Name);
					t.TextInsert(playerTsTable, entry.EntryId.ToBytes(), entry.Name);

					//Committing entry
					t.Commit();
				}

			} catch (Exception ex) {
				throw ex;
			}

		}


		const string remixTable = "Remixes";
		const string remixTsTable = "TS_Remixes";

		void Insert(RemixEntry entry) {

			try {
				//* We are going to store all customers in one table
				//* Later we are going to search customers by their IDs and Names

				using (var t = engine.GetTransaction()) {
					//Documentation https://goo.gl/Kwm9aq
					//* This line with a list of tables we need in case if we modify more than 1 table inside of transaction
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
						//* to Get customer by ID
						// new DBreeze.Objects.DBreezeIndex(1,entry.Id) { PrimaryIndex = true },
						new DBreeze.Objects.DBreezeIndex(1, entry.EntryId) { PrimaryIndex = true },
					}
					}, false);

					Debug.Log("inserted remix: "
						+ entry.EntryId
						+ ", " + entry.RemixId
					);

					//Documentation https://goo.gl/s8vtRG
					//* Setting text search index. We will store text-search 
					//* indexes concerning customers in table "TS_Customers".
					//* Second parameter is a reference to the customer ID.
					t.TextInsert(remixTsTable, entry.EntryId.ToBytes(), entry.RemixId);

					//Committing entry
					t.Commit();
				}

			} catch (Exception ex) {
				throw ex;
			}

		}


		const string highscoreTable = "Highscores";

		void Insert(IEnumerable<HighscoreEntry> highscores) {
			try {
				/*
				 * We are going to store all orders from all customers in one table.
				 * Later we are planning to search orders:
				 *    1. by Order.Id
				 *    2. by Order.udtCreated From-To
				 *    3. by Order.CustomerId and Order.udtCreated From-To
				 */

				using (var t = engine.GetTransaction()) {
					//* This line with a list of tables we need in case if we modify more than 1 table inside of transaction
					//Documentation https://goo.gl/Kwm9aq
					t.SynchronizeTables(highscoreTable);

					foreach (var highscore in highscores) {
						// bool newEntity = order.Id == 0;
						bool newEntity = highscore.EntryId == 0;
						if (newEntity)
							highscore.EntryId = t.ObjectGetNewIdentity<long>(highscoreTable);

						t.ObjectInsert(highscoreTable, new DBreeze.Objects.DBreezeObject<HighscoreEntry> {
							NewEntity = newEntity,
							Indexes = new List<DBreeze.Objects.DBreezeIndex> {
								//* get highscore by ID
								new DBreeze.Objects.DBreezeIndex(1,highscore.EntryId) { PrimaryIndex = true },
								//* get highscore by remix
								new DBreeze.Objects.DBreezeIndex(2,highscore.RemixEntryId),
								//* get highscore by remix and player
								new DBreeze.Objects.DBreezeIndex(3,highscore.RemixEntryId, highscore.PlayerEntryId)

								//* to get highscore in specified time interval
								// new DBreeze.Objects.DBreezeIndex(2,highscore.udtCreated) { AddPrimaryToTheEnd = true }, //* AddPrimaryToTheEnd by default is true
								//* to get highscore in specified time range for specific customer
								// new DBreeze.Objects.DBreezeIndex(3,highscore.RemixEntryId, highscore.udtCreated),
							},
							Entity = highscore  //* Setting entity
						}, false);  //* set last parameter to true, if batch operation speed unsatisfactory
					}

					//* Committing all changes
					t.Commit();
				}
			} catch (Exception ex) {
				throw ex;
			}
		}

		// void Insert(HighscoreEntry highscore) {
		// 	Insert(new HighscoreEntry[] {
		// 		highscore
		// 	});
		// }


		public void Insert(string playerName, string remixId, int score, int time, CharacterSelected character) {

			try {
				using (var t = engine.GetTransaction()) {
					t.SynchronizeTables(
						playerTable,
						playerTsTable,
						remixTable,
						remixTsTable,
						highscoreTable
					);


					// TODO: insert player
					// TODO: check if player exists
					var player = new PlayerEntry() { Name = playerName };
					foreach (var doc in t.TextSearch(playerTsTable).BlockAnd(playerName).GetDocumentIDs()) {
						var obj = t.Select<byte[], byte[]>(playerTable, 1.ToIndex(doc)).ObjectGet<PlayerEntry>();
						if (obj != null) {
							player.EntryId = obj.Entity.EntryId;
						}
					}

					{
						bool newEntity = player.EntryId == 0;
						if (newEntity)
							player.EntryId = t.ObjectGetNewIdentity<long>(playerTable);

						t.ObjectInsert(playerTable, new DBreeze.Objects.DBreezeObject<PlayerEntry> {
							NewEntity = false,
							Entity = player,
							Indexes = new List<DBreeze.Objects.DBreezeIndex> {
								new DBreeze.Objects.DBreezeIndex(1, player.EntryId) { PrimaryIndex = true },
							}
						}, false);

						t.TextInsert(playerTsTable, player.EntryId.ToBytes(), player.Name);
					}

					// TODO: insert remix
					// TODO: check if remix exists
					var remix = new RemixEntry() { RemixId = remixId };
					foreach (var doc in t.TextSearch(remixTsTable).BlockAnd(remixId).GetDocumentIDs()) {
						var obj = t.Select<byte[], byte[]>(remixTable, 1.ToIndex(doc)).ObjectGet<RemixEntry>();
						if (obj != null) {
							remix.EntryId = obj.Entity.EntryId;
						}
					}

					{
						bool newEntity = remix.EntryId == 0;
						if (newEntity)
							remix.EntryId = t.ObjectGetNewIdentity<long>(remixTable);

						//Documentation https://goo.gl/YtWnAJ
						t.ObjectInsert(remixTable, new DBreeze.Objects.DBreezeObject<RemixEntry> {
							// NewEntity = newEntity,
							NewEntity = false,
							Entity = remix,
							Indexes = new List<DBreeze.Objects.DBreezeIndex> {
								new DBreeze.Objects.DBreezeIndex(1, remix.EntryId) { PrimaryIndex = true },
							}
						}, false);

						t.TextInsert(remixTsTable, remix.EntryId.ToBytes(), remix.RemixId);
					}

					// TODO: insert highscore
					// TODO: dont insert highscore if too low (and too many)
					// TODO: delete lowest highscore in db if too many
					var highscore = new HighscoreEntry {
						RemixEntryId = remix.EntryId,
						PlayerEntryId = player.EntryId,
						Score = score,
						Time = time,
						Character = character
					};

					{
						bool newEntity = highscore.EntryId == 0;
						if (newEntity)
							highscore.EntryId = t.ObjectGetNewIdentity<long>(highscoreTable);

						t.ObjectInsert(highscoreTable, new DBreeze.Objects.DBreezeObject<HighscoreEntry> {
							NewEntity = newEntity,
							Indexes = new List<DBreeze.Objects.DBreezeIndex> {
								new DBreeze.Objects.DBreezeIndex(1,highscore.EntryId) { PrimaryIndex = true },
								new DBreeze.Objects.DBreezeIndex(2,highscore.RemixEntryId),
								new DBreeze.Objects.DBreezeIndex(3,highscore.RemixEntryId, highscore.PlayerEntryId)
							},
							Entity = highscore
						}, false);
					}


					t.Commit();
				}
			} catch (Exception ex) {
				throw ex;
			}

		}

		void UpdateHighscore(long orderId) {
			try {

				using (var t = engine.GetTransaction()) {
					//* This line with a list of tables we need in case if we modify more than 1 table inside of transaction
					//Documentation https://goo.gl/Kwm9aq
					t.SynchronizeTables(highscoreTable);

					var highscore = t.Select<byte[], byte[]>(highscoreTable, 1.ToIndex(orderId)).ObjectGet<HighscoreEntry>();
					if (highscore == null)
						return;

					highscore.Entity.udtCreated = new DateTime(1977, 1, 1);
					highscore.Indexes = new List<DBreeze.Objects.DBreezeIndex>() {
						//* to Get order by ID
						new DBreeze.Objects.DBreezeIndex(1, highscore.Entity.EntryId) { PrimaryIndex = true },
						//* get highscore by remix
						new DBreeze.Objects.DBreezeIndex(2,highscore.Entity.RemixEntryId),
						//* get highscore by remix and player
						new DBreeze.Objects.DBreezeIndex(3,highscore.Entity.RemixEntryId, highscore.Entity.PlayerEntryId)

						// //* to get orders in specified time interval
						// new DBreeze.Objects.DBreezeIndex(2, ord.Entity.udtCreated), //* AddPrimaryToTheEnd by default is true
						// //* to get orders in specified time range for specific customer
						// new DBreeze.Objects.DBreezeIndex(3, ord.Entity.RemixEntryId, ord.Entity.udtCreated)
					};

					t.ObjectInsert<HighscoreEntry>(highscoreTable, highscore, false);

					//* Committing all changes
					t.Commit();
				}
			} catch (Exception ex) {
				throw ex;
			}
		}

		public IEnumerable<HighscoreEntry> GetAllHighscores() {
			// try {
			using (var t = engine.GetTransaction()) {
				//Documentation https://goo.gl/MbZAsB
				// foreach (var row in t.SelectForwardFromTo<byte[], byte[]>(highscoreTable,
				foreach (var row in t.SelectForwardFromTo<byte[], byte[]>(highscoreTable,
					1.ToIndex(long.MinValue), true,
					1.ToIndex(long.MaxValue), true
				)) {
					var obj = row.ObjectGet<HighscoreEntry>();
					if (obj != null) {

						// Console.WriteLine(
						// Debug.Log(
						// 	"GetHigscoresByRemix " + remixEntryId + ": "
						// 	+ obj.Entity.EntryId
						// 	+ ", " + obj.Entity.udtCreated.ToString("dd.MM.yyyy HH:mm:ss.fff")
						// 	+ ", " + obj.Entity.RemixEntryId
						// 	+ ", " + obj.Entity.PlayerEntryId
						// 	+ ", " + obj.Entity.Score
						// 	+ ", " + obj.Entity.Time
						// 	+ ", " + obj.Entity.Character
						// );
						yield return obj.Entity;
					}
				}
			}
			// } catch (Exception ex) {
			// 	throw ex;
			// }
		}

		public IEnumerable<HighscoreEntry> GetHighscoresByRemix(long remixEntryId) {
			// try {
			using (var t = engine.GetTransaction()) {
				//Documentation https://goo.gl/MbZAsB
				// foreach (var row in t.SelectForwardFromTo<byte[], byte[]>(highscoreTable,
				foreach (var row in t.SelectForwardFromTo<byte[], byte[]>(highscoreTable,
					2.ToIndex(remixEntryId, long.MinValue), true,
					2.ToIndex(remixEntryId, long.MaxValue), true
					)) {
					var obj = row.ObjectGet<HighscoreEntry>();
					if (obj != null) {

						// Console.WriteLine(
						// Debug.Log(
						// 	"GetHigscoresByRemix " + remixEntryId + ": "
						// 	+ obj.Entity.EntryId
						// 	+ ", " + obj.Entity.udtCreated.ToString("dd.MM.yyyy HH:mm:ss.fff")
						// 	+ ", " + obj.Entity.RemixEntryId
						// 	+ ", " + obj.Entity.PlayerEntryId
						// 	+ ", " + obj.Entity.Score
						// 	+ ", " + obj.Entity.Time
						// 	+ ", " + obj.Entity.Character
						// );
						yield return obj.Entity;
					}
				}
			}
			// } catch (Exception ex) {
			// 	throw ex;
			// }
		}


		public IEnumerable<HighscoreEntry> GetHighscoresByRemixAndPlayer(long remixEntryId, long playerEntryId) {
			// try {
			using (var t = engine.GetTransaction()) {
				foreach (var row in t.SelectForwardFromTo<byte[], byte[]>(highscoreTable,
					3.ToIndex(remixEntryId, playerEntryId, long.MinValue), true,
					3.ToIndex(remixEntryId, playerEntryId, long.MaxValue), true)) {
					var obj = row.ObjectGet<HighscoreEntry>();
					if (obj != null)
						// Console.WriteLine(
						// TODO: get player name and remix ids from tables
						// Debug.Log(
						// 	"GetHighscoresByRemixAndPlayer " + remixEntryId + ", " + playerEntryId + ": "
						// 	+ obj.Entity.EntryId
						// 	+ ", " + obj.Entity.udtCreated.ToString("dd.MM.yyyy HH:mm:ss.fff")
						// 	+ ", " + obj.Entity.RemixEntryId
						// 	+ ", " + obj.Entity.PlayerEntryId
						// 	+ ", " + obj.Entity.Score
						// 	+ ", " + obj.Entity.Time
						// 	+ ", " + obj.Entity.Character
						// );
						yield return obj.Entity;
				}
			}
			// } catch (Exception ex) {
			// 	throw ex;
			// }
		}

		public RemixEntry GetRemixByEntryId(long remixEntryId) {
			// try {
			using (var t = engine.GetTransaction()) {
				var obj = t.Select<byte[], byte[]>(remixTable, 1.ToIndex(remixEntryId)).ObjectGet<RemixEntry>();
				if (obj != null) {

					// Console.WriteLine(
					// Debug.Log(
					// 	"GetRemixByEntryId " + remixEntryId + ": "
					// 	+ obj.Entity.EntryId
					// 	+ ", " + obj.Entity.RemixId
					// );
					return obj.Entity;
				}
				return null;
			}
			// } catch (Exception ex) {
			// 	throw ex;
			// }
		}

		public IEnumerable<RemixEntry> GetRemixByFreeText(string text) {
			// try {
			using (var t = engine.GetTransaction()) {
				// foreach (var doc in t.TextSearch("TS_Customers").BlockAnd(text).GetDocumentIDs()) {
				foreach (var doc in t.TextSearch(remixTsTable).BlockAnd(text).GetDocumentIDs()) {
					var obj = t.Select<byte[], byte[]>(remixTable, 1.ToIndex(doc)).ObjectGet<RemixEntry>();
					if (obj != null) {
						// Console.WriteLine(
						// Debug.Log(
						// 	"GetRemixByFreeText " + text + ": "
						// 	+ obj.Entity.EntryId
						// 	+ ", " + obj.Entity.RemixId
						// );
						yield return obj.Entity;
					}
				}
			}
			// } catch (Exception ex) {
			// 	throw ex;
			// }
		}

		public PlayerEntry GetPlayerByEntryId(long playerEntryId) {
			// try {
			using (var t = engine.GetTransaction()) {
				var obj = t.Select<byte[], byte[]>(playerTable, 1.ToIndex(playerEntryId)).ObjectGet<PlayerEntry>();
				if (obj != null) {
					// Console.WriteLine(
					// Debug.Log(
					// 	"GetRemixByEntryId " + playerEntryId + ": "
					// 	+ obj.Entity.EntryId
					// 	+ ", " + obj.Entity.Name
					// );
					return obj.Entity;
				}
				return null;
			}
			// } catch (Exception ex) {
			// 	throw ex;
			// }
		}

		public IEnumerable<PlayerEntry> GetPlayerByFreeText(string text) {
			// try {
			using (var t = engine.GetTransaction()) {
				// foreach (var doc in t.TextSearch("TS_Customers").BlockAnd(text).GetDocumentIDs()) {
				foreach (var doc in t.TextSearch(playerTsTable).BlockAnd(text).GetDocumentIDs()) {
					var obj = t.Select<byte[], byte[]>(playerTable, 1.ToIndex(doc)).ObjectGet<PlayerEntry>();
					if (obj != null) {
						// Console.WriteLine(
						// Debug.Log(
						// 	"GetRemixByFreeText " + text + ": "
						// 	+ obj.Entity.EntryId
						// 	+ ", " + obj.Entity.Name
						// );
						yield return obj.Entity;
					}
				}
			}
			// } catch (Exception ex) {
			// 	throw ex;
			// }
		}

		public bool Remove(PlayerEntry player) {

			bool wasDeleted;
			try {
				using (var t = engine.GetTransaction()) {
					t.RemoveKey(playerTable, player.EntryId, out wasDeleted);
					// TODO: remove from ts table
					t.Commit();
				}
			} catch (Exception ex) {
				throw ex;
			}

			return wasDeleted;
		}

		public void RemoveHighscore(long entryId) {
			// try {
			// bool wasDeleted;
			using (var t = engine.GetTransaction()) {
				// t.RemoveKey(highscoreTable, entryId, out bool wasDeleted);
				t.ObjectRemove(highscoreTable, 1.ToIndex(entryId));
				t.Commit();
				// return wasDeleted;
			}
			// } catch (Exception ex) {
			// 	throw ex;
			// }
		}

		public void ClearAllTables() {
			// try {
			using (var t = engine.GetTransaction()) {
				t.RemoveAllKeys(playerTable, true);
				t.RemoveAllKeys(playerTsTable, true);
				t.RemoveAllKeys(remixTable, true);
				t.RemoveAllKeys(remixTsTable, true);
				t.RemoveAllKeys(highscoreTable, true);
				t.Commit();
			}
			// } catch (Exception ex) {
			// 	throw ex;
			// }

		}

	}



	// public static HighscoreList List = new HighscoreList();



	// public static string SavePath = Application.dataPath + "/highscore.json";
	// public static string dbPath = Application.dataPath + "/DBR1";

	// TODO: async/coroutine fn for getting entries

	// public static void Init() {

	// }

}
