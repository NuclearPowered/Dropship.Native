{
    "targets": [
        {
            "target_name": "dropship_native_addon",
            "include_dirs": ["<!(node -p \"require('node-addon-api').include_dir\")"],
            "dependencies": ["<!(node -p \"require('node-addon-api').gyp\")"],
            "cflags!": ["-fno-exceptions"],
            "cflags_cc!": ["-fno-exceptions"],
            "defines": ["NAPI_DISABLE_CPP_EXCEPTIONS"],
            "sources": ["main.cpp"],
            'conditions': [
                ['OS=="linux"', {
                    'defines': [
                    'LINUX_DEFINE'
                    ],
                    'include_dirs': [
                    'include/linux',
                    ],
                }],
                ['OS=="win"', {
                    'defines': [
                    'WINDOWS_SPECIFIC_DEFINE'
                    ],
                }, { # OS != "win",
                    'defines': [
                    'NON_WINDOWS_DEFINE',
                    ],
                }]
            ]
        }
    ]
}
