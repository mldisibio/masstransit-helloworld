namespace Models;

public interface IDataContext
{
    GatewayRequest GetModelObjectById(string id);
    void SaveModelObject(GatewayRequest model);
    // convenience method, since we can get the binary content from the GetById method
    byte[] GetModelContentById(string id);
}
