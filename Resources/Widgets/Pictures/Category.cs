using System.Collections.Generic;

namespace Pictures
{
    public class Category
    {
        public string Title;
        public List<string> Files;

        public Category()
        {
            Files = new List<string>();
        }
    }
}