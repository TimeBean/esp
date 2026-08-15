> ls ~/.esp/esp-idf/
add_path.sh          CONTRIBUTING.md  export.sh      LICENSE        ruff.toml             tools/
CMakeLists.txt       docs/            install.bat    pytest.ini     sdkconfig.rename      translate_config.yml
COMPATIBILITY_CN.md  examples/        install.fish*  README_CN.md   SECURITY.md
COMPATIBILITY.md     export.bat       install.ps1    README.md      sgconfig.yml
components/          export.fish      install.sh*    ROADMAP_CN.md  SUPPORT_POLICY_CN.md
conftest.py          export.ps1       Kconfig        ROADMAP.md     SUPPORT_POLICY.md

> source ~/.esp/esp-idf/export.fish

> idf.py -p /dev/ttyACM0 flash

> idf.py -p /dev/ttyACM0 fullclean
