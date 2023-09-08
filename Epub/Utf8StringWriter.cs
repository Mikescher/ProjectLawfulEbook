using System.Text;

namespace ProjectLawfulEbook.Epub;

public class Utf8StringWriter : StringWriter
{
    public override Encoding Encoding => Encoding.UTF8;
}