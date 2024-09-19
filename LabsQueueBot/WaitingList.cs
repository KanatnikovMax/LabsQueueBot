using System;

public class WaitingList
{
	public WaitingList()
	{
        public readonly List<User> _data = new(30);
		
		public int Count { get => _data.Count; }
		public int FindIndex(long id) => _data.FindIndex(id);
		public void Clear() => _data.Clear();
		public void Add(User user) => _data.Add(user);
		public void RemoveAt(int index) => _data.RemoveAt(index);
		public void Skip(int index) => (_data[index], _data[index + 1]) = (_data[index + 1], _data[index]);
}
}
