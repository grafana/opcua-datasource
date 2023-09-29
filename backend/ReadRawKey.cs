using System;
using System.Collections.Generic;
using System.Text;

namespace plugin_dotnet
{
	public sealed class ReadRawKey
	{
		public ReadRawKey(DateTime startTime, DateTime endTime, int maxValues)
		{
			StartTime = startTime;
			EndTime = endTime;
			MaxValues = maxValues;
		}

		public DateTime StartTime { get; }

		public DateTime EndTime { get; }

		public int MaxValues { get; }

		public override bool Equals(object obj)
		{
			ReadRawKey other = obj as ReadRawKey;
			if (other != null)
			{
				return this.StartTime.Equals(other.StartTime) && this.EndTime.Equals(other.EndTime) && this.MaxValues == other.MaxValues;
			}
			return base.Equals(obj);
		}

		public override int GetHashCode()
		{
			return StartTime.GetHashCode() + EndTime.GetHashCode() * 31 + MaxValues * 31 * 31;
		}

	}



	public sealed class ReadProcessedKey
	{
		public ReadProcessedKey(DateTime startTime, DateTime endTime, Opc.Ua.NodeId aggregate, double resampleInterval)
		{
			StartTime = startTime;
			EndTime = endTime;
			Aggregate = aggregate;
			ResampleInterval = resampleInterval;
		}

		public DateTime StartTime { get; }

		public DateTime EndTime { get; }

		public Opc.Ua.NodeId Aggregate { get; }

		public double ResampleInterval { get; }
		public override bool Equals(object obj)
		{
			ReadProcessedKey other = obj as ReadProcessedKey;
			if (other != null)
			{
				return this.StartTime.Equals(other.StartTime) && this.EndTime.Equals(other.EndTime) && this.ResampleInterval.Equals(other.ResampleInterval) && Aggregate.Equals(other.Aggregate);
			}
			return base.Equals(obj);
		}

		public override int GetHashCode()
		{
			return StartTime.GetHashCode() + EndTime.GetHashCode() * 31 + ResampleInterval.GetHashCode() * 31 * 31 + Aggregate.GetHashCode() * 31;
		}

	}

	public sealed class ReadEventKey
	{
		public ReadEventKey(DateTime startTime, DateTime endTime, uint numValuesPerNode)
		{
			StartTime = startTime;
			EndTime = endTime;
			NumValuesPerNode = numValuesPerNode;
		}

		public DateTime StartTime { get; }

		public DateTime EndTime { get; }

		public uint NumValuesPerNode { get; }

		public override bool Equals(object obj)
		{
			ReadEventKey other = obj as ReadEventKey;
			if (other != null)
			{
				return this.StartTime.Equals(other.StartTime) && this.EndTime.Equals(other.EndTime) && this.NumValuesPerNode.Equals(other.NumValuesPerNode);
			}
			return base.Equals(obj);
		}

		public override int GetHashCode()
		{
			return StartTime.GetHashCode() + EndTime.GetHashCode() * 31 + NumValuesPerNode.GetHashCode() * 31 * 31;
		}
	}
}
