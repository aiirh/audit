﻿using Aiirh.Basic.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aiirh.Audit.Internal;

internal class AuditLogEntry : IAuditLogEntry
{
    private AuditLogEntry(ChangeType changeType, string fullPath, string newValue, string oldValue, IDictionary<string, string> propertyNamesMapping, string pathSeparator, string comment)
    {
        var resultFullPathStringBuilder = new StringBuilder(fullPath);
        foreach (var pair in propertyNamesMapping)
        {
            if (fullPath.Contains(pair.Key))
            {
                resultFullPathStringBuilder = resultFullPathStringBuilder.Replace(pair.Key, pair.Value);
            }
        }
        var resultFullPath = resultFullPathStringBuilder.ToString();
        var segments = resultFullPath.Split([pathSeparator], StringSplitOptions.RemoveEmptyEntries).Reverse().ToArray();

        ChangeType = changeType;
        NewValue = newValue;
        OldValue = oldValue;
        Comment = comment;
        FullPath = resultFullPath;
        PathSegments = segments;
        PropertyName = segments.FirstOrDefault();
    }

    public static AuditLogEntry Edit(string fullPath, string newValue, string oldValue, IDictionary<string, string> propertyNamesMapping, string pathSeparator, string comment)
    {
        return new AuditLogEntry(ChangeType.Edit, fullPath, newValue, oldValue, propertyNamesMapping, pathSeparator, comment);
    }

    public static AuditLogEntry Remove(string fullPath, string oldValue, IDictionary<string, string> propertyNamesMapping, string pathSeparator, string comment)
    {
        return new AuditLogEntry(ChangeType.Remove, fullPath, null, oldValue, propertyNamesMapping, pathSeparator, comment);
    }

    public static AuditLogEntry Add(string fullPath, string newValue, IDictionary<string, string> propertyNamesMapping, string pathSeparator, string comment)
    {
        return new AuditLogEntry(ChangeType.Add, fullPath, newValue, null, propertyNamesMapping, pathSeparator, comment);
    }

    public string PropertyName { get; }

    public string FullPath { get; }

    public string[] PathSegments { get; }

    public string OldValue { get; }

    public string NewValue { get; }

    public ChangeType ChangeType { get; }

    public string Comment { get; }
}

internal class AuditLog : IAuditLog
{
    private readonly List<AuditLogEntry> _entries = [];

    public IEnumerable<IAuditLogEntry> Changes => _entries;

    public string Author { get; }

    public DateTime CreatedDate { get; }

    public AuditLog(DateTime createdDate, string author)
    {
        CreatedDate = createdDate;
        Author = author;
    }

    public void AddEntries(IEnumerable<AuditLogEntry> entries)
    {
        _entries.AddRange(entries);
    }

    public void AddEntriesFromAnotherAuditLog(AuditLog auditLog)
    {
        AddEntries(auditLog._entries);
    }

    public void AddEntry(AuditLogEntry entry)
    {
        AddEntries(entry.MakeCollection());
    }
}