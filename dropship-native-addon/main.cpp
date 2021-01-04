#include <napi.h>

using namespace Napi;

#ifdef WINDOWS_SPECIFIC_DEFINE
#include <windows.h>

void *dlopen (const char *filename, int flags)
{
    HINSTANCE hInst;

    hInst= LoadLibraryA(filename);
    if (hInst==NULL) {
        return NULL;
    }
    return hInst;
}

int dlclose (void *handle)
{
    BOOL ok;
    int rc= 0;

    ok= FreeLibrary ((HINSTANCE)handle);
    if (! ok) {
        rc= -1;
    }
    return rc;
}

void *dlsym (void *handle, const char *name)
{
    FARPROC fp;

    fp= GetProcAddress ((HINSTANCE)handle, name);
    if (!fp) {
      return NULL;
    }
    return (void *)(intptr_t)fp;
}

#define PARSING_LIB "./Dropship.Native.dll"
#endif 

#ifdef LINUX_DEFINE
#include <dlfcn.h>
#define PARSING_LIB "./Dropship.Native.so"
#endif

struct ModMetadata
{
  bool success;

  char *assemblyName;
  char *id;
  char *name;
  char *version;
  char *description;
  char *authors;

  ModMetadata(bool s, char *description): success(s), description(description) {}
};

ModMetadata *parse(char *file)
{
  typedef ModMetadata *(*parse_t)(char *);
  char *error;
  
  void *handle = dlopen(PARSING_LIB, 1);
  
  if (!handle)
  {
    ModMetadata metadata(false, "Could not find native parsing library.");
    return &metadata;
  }

  parse_t func = (parse_t) dlsym(handle, "parse");
  if (!func)
  {
    ModMetadata metadata(false, "Could not locate parse function in native library.");
    return &metadata;
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
