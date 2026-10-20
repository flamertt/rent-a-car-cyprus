using System.Text.RegularExpressions;

namespace RentACar.UnitTests.Architecture;

/// <summary>
/// Dosya düzeni: her .cs dosyasında tek üst düzey tip ve dosya adı = tip adı (DTO, komut, işleyici,
/// EF yapılandırması… hepsi kendi dosyasında). EF'in ürettiği Migrations klasörü hariç.
/// </summary>
public sealed partial class SourceLayoutTests
{
    [Fact]
    public void Every_file_declares_exactly_one_top_level_type_named_after_the_file()
    {
        var src = FindSourceRoot();
        var violations = new List<string>();
        foreach (var file in Directory.EnumerateFiles(src, "*.cs", SearchOption.AllDirectories))
        {
            var relative = Path.GetRelativePath(src, file);
            if (relative.Contains($"{Path.DirectorySeparatorChar}Migrations{Path.DirectorySeparatorChar}")
                || relative.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}")
                || relative.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}")
                || Path.GetFileName(file) == "Program.cs")
            {
                continue;
            }

            var names = TopLevelType().Matches(File.ReadAllText(file)).Select(m => m.Groups["name"].Value).ToList();
            var expected = Path.GetFileNameWithoutExtension(file);
            if (names.Count != 1 || names[0] != expected)
            {
                violations.Add($"{relative} → [{string.Join(", ", names)}]");
            }
        }

        Assert.True(violations.Count == 0, "Kural dışı dosyalar:\n" + string.Join("\n", violations));
    }

    private static string FindSourceRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !Directory.Exists(Path.Combine(dir.FullName, "src", "Modules")))
        {
            dir = dir.Parent;
        }

        return Path.Combine(dir?.FullName ?? throw new DirectoryNotFoundException("backend/src bulunamadı"), "src");
    }

    // Satır başında (girintisiz) tip bildirimi: dosya kapsamlı namespace kullanıldığı için üst düzey tipler girintisizdir.
    [GeneratedRegex(@"^(?:public|internal)\s+(?:(?:sealed|static|abstract|partial|readonly)\s+)*(?:class|record struct|record|struct|interface|enum)\s+(?<name>\w+)", RegexOptions.Multiline)]
    private static partial Regex TopLevelType();
}
