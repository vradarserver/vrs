<Query Kind="Program" />

// Quick & dirty LINQPad script to fix the case in RESX file paths without affecting formatting

void Main()
{
    var valueRegex = new Regex(@"<value>(?<relativePath>.*?);");
    var scriptFolder = Path.GetDirectoryName(Util.CurrentQueryPath);

    foreach(var resxFileName in Directory.GetFiles(scriptFolder, "*.resx", SearchOption.AllDirectories)) {
        var resxFolder = Path.GetDirectoryName(resxFileName);
        var content = File.ReadAllLines(resxFileName);
        var changedContent = false;

        for(var lineIdx = 0;lineIdx < content.Length;++lineIdx) {
            var line = content[lineIdx];
            var match = valueRegex.Match(line);

            if(match.Success) {
                var matchGroup = match.Groups["relativePath"];
                var relativePath = matchGroup.Value;
                if(!String.IsNullOrEmpty(relativePath)) {
                    var valueFullPath = Path.Combine(resxFolder, relativePath);

                    if(File.Exists(valueFullPath)) {
                        var correctCasePath = GetExactPathName(valueFullPath);

                        if(!correctCasePath.EndsWith(relativePath)) {
                            var newRelativePath = correctCasePath.Substring(correctCasePath.Length - relativePath.Length);
                            content[lineIdx] = 
                                line.Substring(0, matchGroup.Index) +
                                newRelativePath +
                                line.Substring(matchGroup.Index + matchGroup.Length);

                            if(changedContent) {
                                Console.WriteLine();
                            } else {
                                Console.WriteLine();
                                Console.WriteLine(resxFileName);
                                Console.WriteLine(new String('=', resxFileName.Length));
                            }
                            Console.WriteLine($"OLD:{line}");
                            Console.WriteLine($"NEW:{content[lineIdx]}");
                            changedContent = true;
                        }
                    }
                }
            }
        }

        if(changedContent) {
            Util.ReadLine("Press ENTER to save or stop script to abandon:");
            File.WriteAllLines(resxFileName, content);
        }
    }
}

// Copied from here:
// https://stackoverflow.com/questions/325931/getting-actual-file-name-with-proper-casing-on-windows-with-net
public static string GetExactPathName(string pathName)
{
    if (!(File.Exists(pathName) || Directory.Exists(pathName)))
        return pathName;

    var di = new DirectoryInfo(pathName);

    if (di.Parent != null) {
        return Path.Combine(
            GetExactPathName(di.Parent.FullName), 
            di.Parent.GetFileSystemInfos(di.Name)[0].Name);
    } else {
        return di.Name.ToUpper();
    }
}
    