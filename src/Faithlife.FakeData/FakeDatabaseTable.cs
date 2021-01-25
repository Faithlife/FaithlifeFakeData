using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Faithlife.Reflection;

namespace Faithlife.FakeData
{
	/// <summary>
	/// A fake database table.
	/// </summary>
	/// <typeparam name="T">The type of record stored by the table.</typeparam>
	/// <remarks><para>The owning database must be locked when any method of this class is called.</para>
	/// <para>The first auto ID used by the table is arbitrary to help prevent bugs related to predictable auto IDs.</para></remarks>
	public sealed class FakeDatabaseTable<T> : IEnumerable<T>
	{
		/// <summary>
		/// Adds a record to the table.
		/// </summary>
		/// <remarks>The record object is cloned and assigned the next auto ID before
		/// it is stored. The returned record is a clone of the stored record and can be used
		/// to access the record ID.</remarks>
		public T Add(T record)
		{
			if (record == null)
				throw new ArgumentNullException(nameof(record));

			VerifyContextLocked();

			var newRecord = s_dtoInfo.ShallowClone(record);

			if (m_primaryKey != null)
			{
				if (m_primaryKey is DtoProperty<T, long> longKey)
					TrySetKeyValue(longKey, m_nextAutoId++);
				else if (m_primaryKey is DtoProperty<T, long?> nullableLongKey)
					TrySetKeyValue(nullableLongKey, m_nextAutoId++);
				else if (m_primaryKey is DtoProperty<T, int> intKey)
					TrySetKeyValue(intKey, m_nextAutoId++);
				else if (m_primaryKey is DtoProperty<T, int?> nullableIntKey)
					TrySetKeyValue(nullableIntKey, m_nextAutoId++);

				void TrySetKeyValue<TValue>(DtoProperty<T, TValue> k, TValue v)
				{
					if (EqualityComparer<TValue>.Default.Equals(k.GetValue(newRecord)!, default!))
						k.SetValue(newRecord, v);
				}
			}

			ValidateRecord(newRecord);
			m_records.Add(newRecord);
			return s_dtoInfo.ShallowClone(newRecord);
		}

		/// <summary>
		/// Adds records to the table.
		/// </summary>
		/// <remarks>See <see cref="Add"/>.</remarks>
		public IReadOnlyList<T> AddRange(IEnumerable<T> records)
		{
			if (records == null)
				throw new ArgumentNullException(nameof(records));

			return records.Select(Add).ToList();
		}

		/// <summary>
		/// Updates records from the table that match the specified condition.
		/// </summary>
		/// <returns>The number of records updated.</returns>
		public int UpdateWhere(Func<T, bool> condition, Action<T> action)
		{
			if (condition == null)
				throw new ArgumentNullException(nameof(condition));
			if (action == null)
				throw new ArgumentNullException(nameof(action));

			VerifyContextLocked();

			var updateCount = 0;
			foreach (var record in m_records.Where(condition))
			{
				action(record);
				ValidateRecord(record);
				updateCount++;
			}

			return updateCount;
		}

		/// <summary>
		/// Removes records from the table that match the specified condition.
		/// </summary>
		/// <returns>The number of records removed.</returns>
		public int RemoveWhere(Func<T, bool> condition)
		{
			if (condition == null)
				throw new ArgumentNullException(nameof(condition));

			VerifyContextLocked();

			var removeCount = 0;
			foreach (var record in m_records.Where(condition).ToList())
				removeCount += m_records.Remove(record) ? 1 : 0;
			return removeCount;
		}

		/// <summary>
		/// Enumerates the records in the table.
		/// </summary>
		/// <remarks>The returned records are clones of the actual database records.
		/// Use <see cref="UpdateWhere" /> to make modifications to records.</remarks>
		public IEnumerator<T> GetEnumerator()
		{
			VerifyContextLocked();
			return m_records.Select(x => s_dtoInfo.ShallowClone(x)).GetEnumerator();
		}

		/// <summary>
		/// Gets the next automatic ID for this table.
		/// </summary>
		public int GetNextAutoId()
		{
			VerifyContextLocked();
			return m_nextAutoId;
		}

		/// <summary>
		/// Sets the next automatic ID for this table.
		/// </summary>
		public void SetNextAutoId(int nextAutoId)
		{
			VerifyContextLocked();
			m_nextAutoId = nextAutoId;
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		internal FakeDatabaseTable(FakeDatabaseContext context)
		{
			m_context = context;
			m_records = new HashSet<T>();
			m_nextAutoId = 1;

			var keyProperties = s_dtoInfo.Properties.Where(x => x.MemberInfo.GetCustomAttribute<KeyAttribute>() != null).ToList();
			if (keyProperties.Count == 1)
				m_primaryKey = keyProperties[0];

			var validators = new List<Action<T>>();
			foreach (var property in s_dtoInfo.Properties)
			{
				var propertyInfo = property.MemberInfo;

				var notNull = propertyInfo.GetCustomAttribute<RequiredAttribute>() != null;
				var stringLength = propertyInfo.GetCustomAttribute<StringLengthAttribute>()?.MaximumLength;
				var regexPattern = propertyInfo.GetCustomAttribute<RegularExpressionAttribute>()?.Pattern;
				if (notNull || stringLength != null || regexPattern != null)
				{
					void Validate(T record)
					{
						var value = property.GetValue(record);

						if (value == null)
						{
							if (notNull)
								throw new InvalidOperationException($"{property.Name} must not be null.");
						}
						else if (value is string stringValue)
						{
							if (stringLength != null && stringValue.Length > stringLength.Value)
								throw new InvalidOperationException($"{property.Name} is too long (length {stringValue.Length}, max length {stringLength.Value}).");
							if (regexPattern != null && !Regex.IsMatch(stringValue, regexPattern))
								throw new InvalidOperationException($"{property.Name} does not match the regex '{regexPattern}'.");
						}
					}

					validators.Add(Validate);
				}
			}

			m_validators = validators;
		}

		private void VerifyContextLocked()
		{
			if (!m_context.IsLocked)
				throw new InvalidOperationException("Database context must be locked.");
		}

		private void ValidateRecord(T record)
		{
			foreach (var validator in m_validators)
				validator(record);
		}

		private static readonly DtoInfo<T> s_dtoInfo = DtoInfo.GetInfo<T>();

		private readonly FakeDatabaseContext m_context;
		private readonly HashSet<T> m_records;
		private readonly IDtoProperty<T>? m_primaryKey;
		private readonly IReadOnlyList<Action<T>> m_validators;
		private int m_nextAutoId;
	}
}
