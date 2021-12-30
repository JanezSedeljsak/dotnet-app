namespace Core.Services.PDFGenerator;

public class PDFStringGenerator {
    public static string ActiveUsers() {
        var sb = new StringBuilder();
        sb.Append(@"
            <html>
                <head></head>
                <body>
                    <h1>title</h1>
                    <p>hellou</p>
                </body>
            </html>
        ");
        return sb.ToString();
    }
}