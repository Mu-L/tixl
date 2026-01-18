using System.IO;
using T3.Core.Model;
using T3.Core.Operator;
using T3.Core.Operator.Slots;
using T3.Core.Utils;
using T3.Editor.Gui.InputUi.SimpleInputUis;
using T3.Editor.UiModel;

namespace T3.Editor.Gui.Interaction.StartupCheck;

/// <summary>
/// Validates and updates the asset-paths of all loaded symbols 
/// </summary>
internal static class ConformAssetPaths
{
    // internal static void ConformResourceDirectories()
    // {
    //     foreach (var package in SymbolPackage.AllPackages)
    //     {
    //         if (package.ResourcesFolder.EndsWith("/Resources"))
    //         {
    //             
    //         }
    //     }
    // }
    
    internal static void ConformAllPaths()
    {
        foreach (var package in SymbolPackage.AllPackages)
        {
            foreach (var symbol in package.Symbols.Values)
            {
                SymbolUi symbolUi = null;
                foreach (var inputDef in symbol.InputDefinitions)
                {
                    if (inputDef.ValueType != typeof(string))
                        continue;

                    symbolUi ??= symbol.GetSymbolUi();
                    if (!symbolUi.InputUis.TryGetValue(inputDef.Id, out var inputUi))
                        continue;
                    
                    if (inputUi is not StringInputUi stringUi)
                        continue;
                    
                    if (stringUi.Usage != StringInputUi.UsageType.FilePath 
                        && stringUi.Usage != StringInputUi.UsageType.DirectoryPath)
                        continue;
                        
                    if (inputDef.DefaultValue is not InputValue<string> stringValue)
                        continue;

                    if (TryConvertResourcePath(stringValue.Value, symbol, out string converted))
                    {
                        Log.Debug($"{stringValue.Value} -> {converted}");
                    }
                    
                }
                
                foreach (var child in symbol.Children.Values)
                {
                    foreach (var input in child.Inputs.Values)
                    {
                        if (input.IsDefault)
                            continue;
                        
                        if (input.InputDefinition.ValueType != typeof(string))
                            continue;

                        if (input.Value is not InputValue<string> stringValue)
                            continue;

                        if (string.IsNullOrEmpty(stringValue.Value))
                            continue;

                        if (!SymbolUiRegistry.TryGetSymbolUi(child.Symbol.Id, out var childSymbolUi))
                            continue;

                        if (!childSymbolUi.InputUis.TryGetValue(input.Id, out var inputUi))
                            continue;

                        if (inputUi is not StringInputUi stringUi)
                            continue;

                        if (stringUi.Usage != StringInputUi.UsageType.FilePath 
                            && stringUi.Usage != StringInputUi.UsageType.DirectoryPath)
                            continue;
                        
                        if (TryConvertResourcePath(stringValue.Value, symbol, out string converted))
                        {
                            Log.Debug($"{stringValue.Value} -> {converted}");
                        }
                    }
                }
            }
        }
    }

    private static bool TryConvertResourcePath(string path, Symbol symbol, out string newPath)
    {
        newPath = path;
        if (string.IsNullOrWhiteSpace(path))
            return false;

        // 1. Standardize formatting immediately
        var conformedPath = path.Replace("\\", "/");
    
        // 2. Handle Absolute Paths (No change needed)
        if (Path.IsPathRooted(conformedPath))
        {
            newPath = conformedPath;
            return false;
        }

        var package = symbol.SymbolPackage;
        var packageName = package.Name; 
    
        // 3. Handle Old Aliased/Package paths: "/PackageName/path/..."
        if (conformedPath.StartsWith('/'))
        {
            var trimmed = conformedPath.TrimStart('/');
            var firstSlash = trimmed.IndexOf('/');
        
            if (firstSlash != -1)
            {
                var detectedPackage = trimmed[..firstSlash];
                var subPath = trimmed[(firstSlash + 1)..];
                // Convert to "PackageName:subpath"
                newPath = $"{detectedPackage}:{subPath}";
                return true;
            }
        }

        // 4. Handle Legacy Local paths: "images/test.png"
        // Since we are dropping local support, we MUST prefix them with the current package
        // We also strip any legacy "Resources/" or "Assets/" prefix if it was manually typed
        var legacyPrefix = "Resources/";
        var assetPrefix = "Assets/";
    
        var finalSubPath = conformedPath;
        if (finalSubPath.StartsWith(legacyPrefix, StringComparison.OrdinalIgnoreCase))
            finalSubPath = finalSubPath[legacyPrefix.Length..];
        else if (finalSubPath.StartsWith(assetPrefix, StringComparison.OrdinalIgnoreCase))
            finalSubPath = finalSubPath[assetPrefix.Length..];

        newPath = $"{packageName}:{finalSubPath}";
        return !string.Equals(newPath, path, StringComparison.Ordinal);
    }
}