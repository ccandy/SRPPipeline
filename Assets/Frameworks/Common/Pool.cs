using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pool<T>
{
	public bool AddObject( string key, T obj)
	{
		int Index = -1;

		if (mLookForTable.TryGetValue(key, out Index))
		{
			Debug.LogErrorFormat("Insert To Pool Error, data is exist. key == {0}", key);
			return false;
		}

		if (mEmptyIndexStack.Count == 0)
		{
			mPool.Add(obj);
			Index = mPool.Count - 1;	
		}
		else
		{
			Index = mEmptyIndexStack.Pop();
			mPool[Index] = obj;
		}

		mLookForTable.Add(key, Index);

		return true;
	}

	public bool RemoveObject(string key)
	{
		int Index = -1;
		if (!mLookForTable.TryGetValue(key, out Index))
		{
			return false;
		}

		mLookForTable.Remove(key);
		mPool[Index] = default(T);
		mEmptyIndexStack.Push(Index);

		return true;
	}

	public T GetObject( string key)
	{
		int Index = -1;
		if (!mLookForTable.TryGetValue(key, out Index))
		{
			return default(T);
		}

		return mPool[Index];
	}

	public int GetIndex(string key)
	{
		int Index = -1;
		if (!mLookForTable.TryGetValue(key, out Index))
		{
			return -1;
		}

		return Index;
	}

	public T GetObject(int Index)
	{
		if (Index >= mPool.Count || Index < 0)
			return default(T);

		return mPool[Index];
	}

	public void Clear()
	{
		mLookForTable.Clear();
		mPool.Clear();
		mEmptyIndexStack.Clear();
	}

	public int Count
	{
		get
		{
			return mPool.Count - mEmptyIndexStack.Count;
		}
	}

	/// <summary>
	/// Do not modify this table unless you are sure the lookForTable make the same modify.
	/// </summary>
	public List<T> pool
	{
		get
		{
			return mPool;
		}
	}

	/// <summary>
	/// Do not modify this table unless you are sure the pool make the same modify.
	/// </summary>
	public Dictionary<string, int> lookForTable
	{
		get
		{
			return mLookForTable;
		}
	}

	Dictionary<string, int> mLookForTable = new Dictionary<string, int>();
	List<T> mPool = new List<T>();
	Stack<int> mEmptyIndexStack = new Stack<int>();
}

public class SimplePool<T>
{
	public int AddObject(T obj)
	{
		int index = -1;

		if (mEmptyIndexStack.Count == 0)
		{
			mPool.Add(obj);
			index = mPool.Count - 1;
		}
		else
		{
			index = mEmptyIndexStack.Pop();
			mPool[index] = obj;
		}

		return index;
	}

	public bool RemoveObject(int index)
	{
		if (index < 0 || index >= mPool.Count)
			return false;


		mPool[index] = default(T);
		mEmptyIndexStack.Push(index);

		return true;
	}

	public T GetObject(int Index)
	{
		if (Index >= mPool.Count || Index < 0)
			return default(T);

		return mPool[Index];
	}

	public void Clear()
	{
		mPool.Clear();
		mEmptyIndexStack.Clear();
	}

	public int Count
	{
		get
		{
			return mPool.Count - mEmptyIndexStack.Count;
		}
	}

	/// <summary>
	/// Do not modify this table unless you are sure the lookForTable make the same modify.
	/// </summary>
	public List<T> pool
	{
		get
		{
			return mPool;
		}
	}

	List<T> mPool = new List<T>();
	Stack<int> mEmptyIndexStack = new Stack<int>();
}

public class PoolKV<K,T>
{
    public bool AddObject(K key, T obj)
    {
		int Index = -1;

        if (mLookForTable.TryGetValue( key, out Index))
        {
            Debug.LogErrorFormat("Insert To Pool Error, data is exist. key == {0}", key);
            return false;
        }

        if (mEmptyIndexStack.Count == 0)
        {
            mPool.Add(obj);
            Index = mPool.Count - 1;
        }
        else
        {
            Index = mEmptyIndexStack.Pop();
            mPool[Index] = obj;
        }

        mLookForTable.Add(key, Index);

        return true;
    }

    public bool RemoveObject(K key)
    {
        int Index = -1;
        if (!mLookForTable.TryGetValue(key, out Index))
        {
            return false;
        }

        mLookForTable.Remove(key);
        mPool[Index] = default;
        mEmptyIndexStack.Push(Index);

        return true;
    }

    public T GetObject(K key)
    {
        int Index = -1;
        if (!mLookForTable.TryGetValue(key, out Index))
        {
            return default;
        }

        return mPool[Index];
    }

    public int GetIndex(K key)
    {
        int Index = -1;
        if (!mLookForTable.TryGetValue(key, out Index))
        {
            return -1;
        }

        return Index;
    }

    public T GetObjectFromIndex(int Index)
    {
        if (Index >= mPool.Count || Index < 0)
            return default(T);

        return mPool[Index];
    }

    public void Clear()
    {
        mLookForTable.Clear();
        mPool.Clear();
        mEmptyIndexStack.Clear();
    }

    public int Count
    {
        get
        {
            return mPool.Count - mEmptyIndexStack.Count;
        }
    }

    /// <summary>
    /// Do not modify this table unless you are sure the lookForTable make the same modify.
    /// </summary>
    public List<T> pool
    {
        get
        {
            return mPool;
        }
    }

    /// <summary>
    /// Do not modify this table unless you are sure the pool make the same modify.
    /// </summary>
    public Dictionary<K, int> lookForTable
    {
        get
        {
            return mLookForTable;
        }
    }

    Dictionary<K, int> mLookForTable = new Dictionary<K, int>();
    List<T> mPool = new List<T>();
    Stack<int> mEmptyIndexStack = new Stack<int>();
}

public interface OrderLessObj
{
	int CurOrderlessIndex();
	void SetOrderlessIndex(int index);
}

public class OrderlessPool<T>  where T : OrderLessObj, new()
{
	List<T> mPool = new List<T>();

	int mCount = 0;

	public int Count
	{
		get
		{
			return mCount;
		}
	}

	public void Insert(T obj)
	{
		// the data in this pool can't be null
		if (obj == null)
			return;

		if (Count < mPool.Count)
		{
			mPool[mCount] = obj;
			obj.SetOrderlessIndex(mCount);
			++mCount;
			return;
		}

		mPool.Add(obj);
		obj.SetOrderlessIndex(mCount);
		++mCount;
	}

	public void Remove(T obj)
	{
		if (obj == null)
			return;

		int removeIndex = obj.CurOrderlessIndex();

		if ( !mPool[removeIndex].Equals(obj) )
		{
			return;
		}

		mPool[removeIndex] = default(T);
		if (mCount == 0)
		{	
			return;
		}

		if (removeIndex != mCount - 1)
		{
			mPool[removeIndex] = mPool[mCount-1];
			mPool[removeIndex].SetOrderlessIndex(removeIndex);
			mPool[mCount-1] = default(T);
		}
		--mCount;
	}

	public void RemoveIndex(int removeIndex)
	{
		if (mPool.Count <= removeIndex || removeIndex < 0)
			return;

		mPool[removeIndex] = default(T);
		if (mCount == 0)
		{
			return;
		}

		mPool[removeIndex] = mPool[mCount];
		mPool[removeIndex].SetOrderlessIndex(removeIndex);
		mPool[mCount] = default(T);
		--mCount;
	}

	public void Clear()
	{
		mPool.Clear();
		mCount = 0;
	}

	public T this[int index]
	{
		get
		{
			return mPool[index];
		}
	}
}