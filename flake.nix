{
  inputs = {
    nixpkgs.url = "github:NixOS/nixpkgs/nixos-unstable";
  };

  outputs =
    { nixpkgs, ... }:
    let
      forAllSystems = nixpkgs.lib.genAttrs nixpkgs.lib.systems.flakeExposed;
    in
    {
      devShells = forAllSystems (
        system:
        let
          pkgs = nixpkgs.legacyPackages.${system};
        in
        {
          default = pkgs.mkShell {
            packages = [
              pkgs.dotnetCorePackages.sdk_10_0
              (pkgs.writeShellApplication {
                name = "dev-update-js-yaml";
                text = ''
                  "${pkgs.curl}/bin/curl" -fsSL \
                    "https://cdn.jsdelivr.net/npm/js-yaml/dist/browser/js-yaml.umd.min.js" \
                    -o "src/MihomoManager.MihomoMixin/Js/js-yaml.umd.min.js"
                '';
              })
              (pkgs.writeShellApplication {
                name = "dev-publish";
                text = ''
                  mkdir -p publish
                  dotnet pack src/MihomoManager.MihomoMixin/MihomoManager.MihomoMixin.csproj -c Release -o publish
                '';
              })
            ];
            shellHook = ''
              export DOTNET_ROOT="${pkgs.dotnetCorePackages.sdk_10_0}/share/dotnet"
            '';
          };
        }
      );
    };
}
