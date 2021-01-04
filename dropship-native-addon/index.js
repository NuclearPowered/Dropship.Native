const addon = require('bindings')('dropship_native_addon');

try {
    console.log(addon.parse("./Example.dll"));
} catch (e) {
    console.warn(e);
}
