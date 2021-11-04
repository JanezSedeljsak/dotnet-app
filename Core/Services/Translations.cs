namespace Core.Services.Translations;

public class TranslateService {
    // cache for translations {<langCode>_<classObj>: <key>: [actual translation]}}
    public Dictionary<string, Dictionary<string, string>> cachedTranslatios { get; set; }
    public TranslateService() {
        cachedTranslatios = new Dictionary<string, Dictionary<string, string>>();
    }
    public Dictionary<string, string> getClassObj(string objKey) {
        if (cachedTranslatios.TryGetValue(objKey, out var translationObj)) {
            return translationObj;
        }
        // @TODO: read file and lone translation obj
        var objFromFile = new Dictionary<string, string>(); 
        cachedTranslatios.Add(objKey, objFromFile);
        return objFromFile;
    }

    public string get(string baseClass, string key, string countryCode="en") {
        var objKey = $"{countryCode}_{baseClass}";
        var translationsObj = this.getClassObj(objKey);
        if (translationsObj.TryGetValue(key, out var translation)) {
            return translation;
        }

        return key;
    }
}