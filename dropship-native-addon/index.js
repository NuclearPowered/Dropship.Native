const addon = require('bindings')('dropship_native_addon');
const os = require('os')
const path = require('path')
const parse = (userpath) => {
    if (os.platform() === 'win32') {
        const dllPath = path.join(__dirname, "Dropship.Native.dll")
        return addon.parse(userpath, dllPath)
    } else if (os.platform() === 'linux') {
        const soPath = path.join(__dirname, "Dropship.Native.so")
        return addon.parse(userpath, soPath)
    } else {
        throw new Error("Unsupported OS for Dropship.Native")
    }
};
exports.parse = parse