using System;

namespace CS422
{
	class Node
	{
		int data;

		public Node Next
		{
			get; set;
		}

		public int Data
		{
			get
			{
				return data;
			}
		}


		public Node(int data)
		{
			this.data = data;
		}
	}

	public class PCQueue
	{
		Node front = null;
		Node back = null;

		public PCQueue()
		{
			// Point to a dummy node
			front = back = new Node(0);
		}

		public bool Dequeue(ref int data)
		{
			// If the queue is empty
			if (front.Equals(back))
			{
				return false;
			}

			// Give callee data
			data = front.Next.Data;

			// Chop off head
			front = front.Next;

			return true;
		}

		public void Enqueue(int data)
		{
			// Create the new node
			Node newNode = new Node(data);
	
			if (front.Equals(back))
			{ // We're empty
				front.Next = newNode;
				back = front.Next;
			}
			else
			{// Put it in the back
				back.Next = newNode;

				// Update back pointer
				back = back.Next;
			}
		}
	}
}