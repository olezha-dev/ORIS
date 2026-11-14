using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    internal class HtmlTeamplateReader : IHtmlTemplateRenderer
    {
        public string RenderFromFile(string htmlTeamplate, object dataModel)
        {
            throw new NotImplementedException();
        }
        public string RenderFromString(string htmlTeamplate, object dataModel)
        {
            Type mType = dataModel.GetType();
            Regex regex = new Regex("\\${\\$*}");
            regax.replace(htmlTeamplate, (match) =>
                string fieldName = Match.Group[1].Value.Split(".")[1]
            );
        }
    }

}
