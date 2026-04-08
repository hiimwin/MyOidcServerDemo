using OidcServer.Models;

namespace OidcServer.Repositories
{
    public class CodeItemRepository : ICodeItemRepository
    {
        private readonly Dictionary<string, CodeItem> _codeItems = new();
        public void Add(string code, CodeItem codeItem)
        {
            _codeItems[code] = codeItem;
        }

        public void Delete(string code)
        {
            _codeItems.Remove(code);
        }

        public CodeItem? FindByCode(string code)
        {
            return _codeItems.TryGetValue(code, out var codeItem) ? codeItem : null;
        }
    }
}
