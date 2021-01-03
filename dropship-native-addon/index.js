const addon = require('bindings')('dropship_native_addon');

try {
    console.log(addon.parse("/mnt/Files/Development/Reactor/Dropship.Native/Dropship.Native.Test/Example.dll"));
} catch (e) {
    console.warn(e);
}
