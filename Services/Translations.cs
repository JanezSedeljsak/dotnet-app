namespace Services.Translations;

public class Translate {

    // cache for translations {<langCode>_<classObj>: <key>: [actual translation]}}
    public Dictionary<string, Dictionary<string, string>> cachedTranslatios { get; set; }
    public Translate() {
        cachedTranslatios = new Dictionary<string, Dictionary<string, string>>();
    }
    public Dictionary<string, string> getClassObj(string objKey) {
        return new Dictionary<string, string>();
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