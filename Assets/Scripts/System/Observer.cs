public interface IObserver<T> {
	void Notify(T subject);
}

public interface IObserver<T1,T2> {
	void Notify(T1 subject1, T2 subject2);
}

public interface IObserver<T1,T2, T3> {
	void Notify(T1 subject1, T2 subject2, T3 subject3);
}
