using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;

namespace CS422
{
	#region Abstracts

	public abstract class Dir422
	{
		public abstract string Name { get; }

		public abstract Dir422 Parent { get; }

		public abstract IList<Dir422> GetDirs();

		public abstract IList<File422> GetFiles();

		public abstract bool ContainsDir(string name, bool recursive);

		public abstract bool ContainsFile(string name, bool recursive);

		public abstract Dir422 GetDir(string name);

		public abstract Dir422 CreateDir(string name);

		public abstract File422 GetFile(string name);

		public abstract File422 CreateFile(string name);
	}

	public abstract class File422
	{
		public File422() { }

		public abstract string Name { get; }

		public abstract Dir422 Parent { get; }

		public abstract Stream OpenReadOnly();

		public abstract Stream OpenReadWrite();
	}

	public abstract class FileSys422
	{
		public abstract Dir422 GetRoot();

		public virtual bool Contains(Dir422 dir)
		{
			if (dir.Name.Contains("/") || dir.Name.Contains("\\") || dir == null)
			{
				return false;
			}

			if (GetRoot() == dir)
			{
				return true;
			}

			return Contains(dir.Parent);
		}

		public virtual bool Contains(File422 file)
		{
			if (file.Name.Contains("/") || file.Name.Contains("\\") || file == null)
			{
				return false;
			}

			return Contains(file.Parent);
		}
	}

	#endregion

	#region Standard

	public class StdFSFile : File422
	{
		string name;
		Dir422 parent;

		public StdFSFile(string name, Dir422 parent = null)
		{
			this.parent = parent;
			this.name = name;
		}

		public override string Name
		{
			get
			{
				return name;
			}
		}

		public override Dir422 Parent
		{
			get
			{
				return parent;
			}
		}

		public override Stream OpenReadOnly()
		{
			try
			{
				return File.OpenRead(name);
			}
			catch (Exception)
			{
				return null;
			}
		}

		public override Stream OpenReadWrite()
		{
			try
			{
				return File.Open(name, FileMode.Open);
			}
			catch (Exception)
			{
				return null;
			}
		}
	}

	public class StdFSDir : Dir422
	{
		string name;
		Dir422 parent;

		public StdFSDir(string name, Dir422 parent = null)
		{
			this.name = name;
			this.parent = parent;
		}

		bool Validity(string str)
		{
			return !(string.IsNullOrEmpty(str) || str.Contains("/") || str.Contains("\\"));
		}

		public override string Name
		{
			get
			{
				return name;
			}
		}

		public override Dir422 Parent
		{
			get
			{
				return parent;
			}
		}

		public override bool ContainsFile(string name, bool recursive)
		{
			if (!recursive)
			{
				return File.Exists(name);
			}

			// Recursive
			if (File.Exists(name))
			{
				return true;
			}

			foreach (var dir in GetDirs())
			{
				if (dir.ContainsFile(name, true))
				{
					return true;
				}
			}

			return false;
		}

		public override bool ContainsDir(string name, bool recursive)
		{
			if (!recursive)
			{
				return Directory.Exists(name);
			}

			// Recursive
			if (Directory.Exists(name))
			{
				return true;
			}

			foreach (var dir in GetDirs())
			{
				if (dir.ContainsDir(name, true))
				{
					return true;
				}
			}

			return false;
		}

		public override Dir422 CreateDir(string name)
		{
			if (!Validity(name) || File.Exists(name))
			{
				return null;
			}

			// Does it already exist?
			if (Directory.Exists(name))
			{
				return new StdFSDir(name, this);
			}

			// Make it
			Directory.CreateDirectory(name);

			return new StdFSDir(name, this);
		}

		public override File422 CreateFile(string name)
		{
			if (!Validity(name) || Directory.Exists(name))
			{
				return null;
			}

			try
			{
				File.Create(name);

				return new StdFSFile(name, this);
			}
			catch
			{
				return null;
			}
		}

		public override Dir422 GetDir(string name)
		{
			if (!Validity(name))
			{
				return null;
			}

			return new StdFSDir(Directory.GetDirectories(Name, name)[0], this);
		}

		public override IList<Dir422> GetDirs()
		{
			List<Dir422> dirs = new List<Dir422>();

			foreach (var dir in Directory.GetDirectories(Name))
			{
				dirs.Add(new StdFSDir(dir, this));
			}

			return dirs;
		}

		public override File422 GetFile(string name)
		{
			if (!Validity(name))
			{
				return null;
			}

			return new StdFSFile(Directory.GetFiles(Name, name)[0], this);
		}

		public override IList<File422> GetFiles()
		{
			List<File422> files = new List<File422>();

			foreach (var file in Directory.GetFiles(Name))
			{
				files.Add(new StdFSFile(file, this));
			}

			return files;
		}
	}

	public class StandardFileSystem : FileSys422
	{
		Dir422 root;

		public StandardFileSystem(string dir)
		{
			root = new StdFSDir(dir);
		}

		public override Dir422 GetRoot()
		{
			return root;
		}

		/*
		 * Creates new file system if directory exists
		 */
		public static StandardFileSystem Create(string root)
		{
			return (Directory.Exists(root)) ? new StandardFileSystem(root) : null;
		}
	}

	#endregion

	#region Memory

	public class MemFSFile : File422
	{
		string name;
		Dir422 parent;
		MemoryStream readStream, readWriteStream;
		bool isReadStreamUsed, isReadWriteStreamUsed;
		byte[] buffer = new byte[4096];

		public MemFSFile(string name, Dir422 parent)
		{
			this.name = name;
			this.parent = parent;

			readStream = new MemoryStream(buffer, false);
			readWriteStream = new MemoryStream(buffer);
		}

		public override string Name
		{
			get
			{
				return name;
			}
		}

		public override Dir422 Parent
		{
			get
			{
				return parent;
			}
		}

		public override Stream OpenReadOnly()
		{
			// Wait to get lock for read-write stream
			lock (isReadWriteStreamUsed as object)
			{
				if (isReadWriteStreamUsed)
				{
					return null;
				}

				lock (isReadStreamUsed as object)
				{
					isReadStreamUsed = true;
					return readStream;
				}
			}
		}

		public override Stream OpenReadWrite()
		{
			try
			{
				// We don't want any read streams open so stop the read stream
				lock (isReadStreamUsed as object)
				{
					lock (isReadWriteStreamUsed as object)
					{
						if (isReadStreamUsed || isReadWriteStreamUsed)
						{
							// Lets see if readStream is disposed of.
							try
							{
								readStream.Seek(0, SeekOrigin.Current);
							}
							catch (ObjectDisposedException)
							{// it is
								isReadStreamUsed = false;

								// Now lets see if readWriteStream is open
								try
								{
									readWriteStream.Seek(0, SeekOrigin.Current);
								}
								catch (ObjectDisposedException)
								{// it is
								 // But now that we know the read streams are disposed and the write stream is disposed we can return the new write stream.
									isReadWriteStreamUsed = true;
									return readWriteStream;
								}
							}

							return null;
						}
					}
				}
			}
			catch (Exception) { }
			return null;
		}
	}

	public class MemFSDir : Dir422
	{
		ConcurrentDictionary<string, Dir422> directories;
		ConcurrentDictionary<string, File422> files;
		string name;
		Dir422 parent;

		public MemFSDir(string name, Dir422 parent = null)
		{
			this.name = name;
			this.parent = parent;

			directories = new ConcurrentDictionary<string, Dir422>();
			files = new ConcurrentDictionary<string, File422>();
		}

		bool Validity(string str)
		{
			return !(string.IsNullOrEmpty(str) || str.Contains("/") || str.Contains("\\"));
		}

		public override string Name
		{
			get
			{
				return name;
			}
		}

		public override Dir422 Parent
		{
			get
			{
				return parent;
			}
		}

		public override bool ContainsDir(string name, bool recursive)
		{
			if (!Validity(name))
			{
				return false;
			}

			if (!recursive)
			{
				return directories.ContainsKey(name);
			}

			// Recursive
			if (directories.ContainsKey(name))
			{
				return true;
			}

			foreach (var dir in GetDirs())
			{
				if (dir.ContainsDir(name, true))
				{
					return true;
				}
			}

			return false;
		}

		public override bool ContainsFile(string name, bool recursive)
		{
			if (!Validity(name))
			{
				return false;
			}

			if (!recursive)
			{
				return files.ContainsKey(name);
			}

			// Recursive
			if (files.ContainsKey(name))
			{
				return true;
			}

			foreach (var dir in GetDirs())
			{
				if (dir.ContainsFile(name, true))
				{
					return true;
				}
			}

			return false;
		}

		public override Dir422 CreateDir(string name)
		{
			if (!Validity(name) || files.ContainsKey(name))
			{
				return null;
			}

			// Make it
			return (directories.ContainsKey(name)) ? directories[name] : new MemFSDir(name, this);
		}

		public override File422 CreateFile(string name)
		{
			if (!Validity(name) || files.ContainsKey(name))
			{
				return null;
			}

			// Make it
			return (files.ContainsKey(name)) ? files[name] : new MemFSFile(name, this);
		}

		public override Dir422 GetDir(string name)
		{
			return (directories.ContainsKey(name)) ? directories[name] : null;
		}

		public override IList<Dir422> GetDirs()
		{
			Dir422[] dirs = new Dir422[directories.Values.Count];

			directories.Values.CopyTo(dirs, 0);

			return dirs;
		}

		public override File422 GetFile(string name)
		{
			return (files.ContainsKey(name)) ? files[name] : null;
		}

		public override IList<File422> GetFiles()
		{
			File422[] f = new File422[files.Values.Count];

			files.Values.CopyTo(f, 0);

			return f;
		}
	}

	public class MemoryFileSystem : FileSys422
	{
		Dir422 root;

		public MemoryFileSystem()
		{
			root = new MemFSDir("root");
		}

		public override Dir422 GetRoot()
		{
			return root;
		}
	}

	#endregion
}
