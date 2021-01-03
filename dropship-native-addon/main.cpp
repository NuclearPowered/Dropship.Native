#include <napi.h>
#include <dlfcn.h>

using namespace Napi;

struct ModMetadata
{
  bool success;

  char *assemblyName;
  char *id;
  char *name;
  char *version;
  char *description;
  char *authors;
};

ModMetadata *parse(char *file)
{
  typedef ModMetadata *(*parse_t)(char *);
  char *error;
  void *handle = dlopen("/mnt/Files/Development/Reactor/Dropship.Native/Dropship.Native/bin/Release/netstandard2.1/linux-x64/publish/Dropship.Native.so", RTLD_LAZY);
  if (!handle)
  {
    fprintf(stderr, "%s\n", dlerror());
    exit(1);
  }
  dlerror();
  parse_t func = (parse_t)dlsym(handle, "parse");
  if ((error = dlerror()) != nullptr)
  {
    fprintf(stderr, "%s\n", error);
    exit(1);
  }

  ModMetadata *result = (*func)(file);
  dlclose(handle);
  return result;
}

Value Parse(const CallbackInfo &info)
{
  Env env = info.Env();

  if (info.Length() != 1)
  {
    TypeError::New(env, "Wrong number of arguments").ThrowAsJavaScriptException();
    return env.Null();
  }

  if (!info[0].IsString())
  {
    TypeError::New(env, "Wrong arguments").ThrowAsJavaScriptException();
    return env.Null();
  }

  char *file = strdup(info[0].As<String>().Utf8Value().c_str());

  ModMetadata *metadata = parse(file);

  if (!metadata->success)
  {
    Error::New(env, metadata->description).ThrowAsJavaScriptException();
    return env.Null();
  }

  Object returnValue = Object::New(env);

  returnValue.Set("assemblyName", String::New(env, metadata->assemblyName));
  returnValue.Set("id", String::New(env, metadata->id));
  returnValue.Set("name", String::New(env, metadata->name));
  returnValue.Set("version", String::New(env, metadata->version));
  returnValue.Set("description", String::New(env, metadata->description));
  returnValue.Set("authors", String::New(env, metadata->authors));

  return returnValue;
}

Object Init(Env env, Object exports)
{
  exports.Set(String::New(env, "parse"), Function::New(env, Parse));

  return exports;
}

NODE_API_MODULE(NODE_GYP_MODULE_NAME, Init)
