namespace Services.Response;

class StatusResponse {
    public bool status { get; set; }
    public string message { get; set; }
    public StatusResponse(bool s, string m) {
        status = s;
        message = m;
    }
}