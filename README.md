# MihomoManager.MihomoMixin

## As a Command-Line Tool

### Installation

1. Install [.NET](https://dotnet.microsoft.com/)
2. Download the zip file from [Releases](https://github.com/MihomoManager/MihomoManager.MihomoMixin/releases/latest) and extract it.

### Quick Start

Prepare a configuration file, e.g. `configuration.yaml`:

```yaml
allow-lan: true
proxies:
- name: "direct"
  type: direct
```

Prepare another configuration file for merging, e.g. `mixin.yaml`:

```yaml
mixed-port: 7890
allow-lan: false
proxies:
- name: "direct2"
  type: direct
```

Run:

```sh
dotnet MihomoManager.MihomoMixin.dll merge configuration.yaml merge mixin.yaml save output.yaml
```

You will get an `output.yaml` like:

```yaml
allow-lan: false
proxies:
  -
    name: direct2
    type: direct
  -
    name: direct
    type: direct
mixed-port: 7890
```

### Actions

#### save

`save` persists the current in-memory configuration to a YAML file at the specified path. It can be used at any point in a pipeline — not only as the final action, but also to capture intermediate state for inspection or checkpointing:

```sh
dotnet MihomoManager.MihomoMixin.dll save 0.yaml edit edit1.yaml save 1.yaml edit edit2.yaml save 2.yaml
```

#### merge

`merge` loads and merges a Mihomo configuration into the current one. The initial configuration is `{}`, so `merge` is also used to load the first configuration file.

```sh
dotnet MihomoManager.MihomoMixin.dll merge shared-config.yaml merge my-config.yaml save merged.yaml
```

The newer configuration takes higher priority.

Every node is categorized into one of three types: **mapping**, **sequence**, and **value**.

- If both the existing configuration and the new configuration are mappings, they are merged.
- If both are sequences, they are merged.
- Otherwise (type mismatch or both are values), the new configuration overwrites the old one.

For **mapping** merging, each key is processed recursively. Keys present in only one of the two configurations are preserved.

For **sequence** merging, items from the new configuration are placed first, followed by items from the existing configuration. If an item is a mapping and has a `name` field, items with the same `name` are merged. If an item is a value, duplicates are removed.

#### edit

`edit` allows modifying rules, proxies, and proxy groups in a structured way. A no-op edit configuration looks like this (note that all arrays must be present even if empty):

```yaml
Rules:
  Delete: []
  Prepend: []
  Append: []
Proxies:
  Delete: []
  Prepend: []
  Append: []
ProxyGroups:
  Delete: []
  Prepend: []
  Append: []
```

To delete a proxy node `Hong Kong 1` and add a direct-connection proxy, use `edit-proxies.yaml`:

```yaml
Rules:
  Delete: []
  Prepend: []
  Append: []
Proxies:
  Delete:
    - "Hong Kong 1"
  Prepend:
    - name: my-direct
      type: direct
  Append: []
ProxyGroups:
  Delete: []
  Prepend: []
  Append: []
```

Then edit rules to route Baidu traffic through the new node, e.g. `edit-rules.yaml`:

```yaml
Rules:
  Delete:
    - "DOMAIN-SUFFIX,google.com,Hong Kong 1"
  Prepend:
    - DOMAIN-SUFFIX,baidu.com,my-direct
  Append: []
Proxies:
  Delete: []
  Prepend: []
  Append: []
ProxyGroups:
  Delete: []
  Prepend: []
  Append: []
```

Apply both edits to `original-config.yaml`:

```sh
dotnet MihomoManager.MihomoMixin.dll merge original-config.yaml edit edit-proxies.yaml edit edit-rules.yaml save edited.yaml
```

#### js

`js` enables JavaScript scripting for more complex configuration transformations. For example, `script.js`:

```js
function main(config, log) {
    log("Hello World!");
    config["rules"] = config["rules"].concat(config["rules"]);
    return config;
}
```

This is similar to the [Clash Verge script feature](https://www.clashverge.dev/guide/script.html), but with key differences in how it works under the hood: it uses the Jint engine to run JavaScript in C#, and the `config` parameter is a dictionary deserialized by SharpYaml. The second parameter `log` prints strings to the standard error stream for debugging, rather than being a configuration name.

Run with:

```sh
dotnet MihomoManager.MihomoMixin.dll merge config.yaml js script.js save new-config.yaml
```

## As a .NET Library

The package is uploaded to [nuget.org](https://www.nuget.org/packages/MihomoManager.MihomoMixin). You could find the actions above and call their `MixinAsync`.

However, as we haven't yet completed strong type annotations for mihomo configuration files, this package provides only very limited support. It is mainly used in conjunction with `dotnet run file.cs` to avoid the hassle of directly calling the CLI.

### Samples

#### Sample 1: Download a configuration file, change its listening port and start mihomo

```csharp
#:package MihomoManager.MihomoMixin@0.2.2

using System.Diagnostics;
using MihomoManager.MihomoMixin.Merge;

var mihomoPath = "./mihomo-windows-amd64-v3.exe";
var configurationUrl = "https://xxxxxxxxxxxxxxxxxxxxxxxxxxxx";
var port = 7899;


var tempFile = new FileInfo(Path.GetTempFileName());

// 1. download the configuration
// You may manually check the response format. (Many providers do return different content based on the User Agent.)
using var http = new HttpClient();
http.DefaultRequestHeaders.UserAgent.ParseAdd("clash-verge/v2.5.1");
var configuration = await http.GetStringAsync(configurationUrl);

// 2. override its listening port
await File.WriteAllTextAsync(tempFile.FullName,
    $"""
    mixed-port: {port}
    """
);
configuration = await new MergeAction(tempFile.FullName).MixinAsync(configuration);

// 3. start mihomo
await File.WriteAllTextAsync(tempFile.FullName, configuration);
await Process.Start(mihomoPath, ["-f", tempFile.FullName]).WaitForExitAsync();
```

