using System.Collections.ObjectModel;
using FindEye.Core.Models;

namespace FindEye.Infrastructure.Helpers;

public static class TreeBuilder
{
    public static List<CompareTreeNode> BuildTree(CompareResult result)
    {
        var allNodes = new Dictionary<string, CompareTreeNode>(StringComparer.OrdinalIgnoreCase);
        var rootNodes = new List<CompareTreeNode>();

        // Create file nodes
        foreach (var (relPath, diffType) in result.DiffMap)
        {
            var sourceDict = result.FilesInA.ContainsKey(relPath) ? result.FilesInA : result.FilesInB;
            sourceDict.TryGetValue(relPath, out var fileInfo);

            var node = new CompareTreeNode
            {
                Name = Path.GetFileName(relPath),
                RelativePath = relPath,
                IsDirectory = false,
                DiffType = diffType,
                Size = fileInfo?.Size ?? 0,
                Sha256Hash = fileInfo?.Sha256Hash,
                LastWriteTime = fileInfo?.LastWriteTime ?? default,
                ErrorMessage = fileInfo?.ErrorMessage
            };
            allNodes[relPath] = node;
        }

        // Build directory hierarchy — snapshot keys to avoid modifying during enumeration
        var filePaths = allNodes.Keys.ToList();
        foreach (var relPath in filePaths)
        {
            var node = allNodes[relPath];
            var dirPath = Path.GetDirectoryName(relPath);
            if (string.IsNullOrEmpty(dirPath))
            {
                rootNodes.Add(node);
            }
            else
            {
                var parentDir = GetOrCreateDirectoryNode(dirPath, allNodes);
                parentDir.Children.Add(node);
            }
        }

        // Aggregate directory diff types and sort
        foreach (var node in allNodes.Values.Where(n => n.IsDirectory))
        {
            AggregateDirectoryDiffType(node);
            SortChildren(node);
        }

        return rootNodes
            .OrderByDescending(n => n.IsDirectory)
            .ThenBy(n => n.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static CompareTreeNode GetOrCreateDirectoryNode(
        string dirPath, Dictionary<string, CompareTreeNode> allNodes)
    {
        if (allNodes.TryGetValue(dirPath, out var existing))
            return existing;

        var parentDir = Path.GetDirectoryName(dirPath);
        var node = new CompareTreeNode
        {
            Name = Path.GetFileName(dirPath),
            RelativePath = dirPath,
            IsDirectory = true,
            DiffType = DiffType.Identical
        };
        allNodes[dirPath] = node;

        if (string.IsNullOrEmpty(parentDir))
        {
            // Root-level directory — not added to rootNodes here;
            // it will be added when a file under it is attached
        }
        else
        {
            var parent = GetOrCreateDirectoryNode(parentDir, allNodes);
            parent.Children.Add(node);
        }

        return node;
    }

    private static void AggregateDirectoryDiffType(CompareTreeNode dirNode)
    {
        var childTypes = dirNode.Children
            .Select(c => c.IsDirectory ? c.DiffType : c.DiffType)
            .Where(t => t != DiffType.Identical)
            .Distinct()
            .ToList();

        if (childTypes.Count == 0)
            dirNode.DiffType = DiffType.Identical;
        else if (childTypes.Contains(DiffType.ContentDifferent))
            dirNode.DiffType = DiffType.ContentDifferent;
        else if (childTypes.Contains(DiffType.OnlyInA) && childTypes.Contains(DiffType.OnlyInB))
            dirNode.DiffType = DiffType.ContentDifferent;
        else if (childTypes.Contains(DiffType.OnlyInA))
            dirNode.DiffType = DiffType.OnlyInA;
        else
            dirNode.DiffType = DiffType.OnlyInB;
    }

    private static void SortChildren(CompareTreeNode dirNode)
    {
        var sorted = dirNode.Children
            .OrderByDescending(c => c.IsDirectory)
            .ThenBy(c => c.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        dirNode.Children = new ObservableCollection<CompareTreeNode>(sorted);
    }
}
