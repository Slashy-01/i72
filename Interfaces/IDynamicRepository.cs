using I72_Backend.Model;
using System.Collections.Generic;

namespace I72_Backend.Interfaces
{
    public interface IDynamicRepository
    {
        ICollection<Dynamic> GetDynamic();

		Dynamic GetUserByDynamic(string name);

        Dynamic GetDynamicById(int id);

        void AddDynamic(Dynamic dynamic);

        void DeleteDynamic(Dynamic dynamic);

        void UpdateDynamic(Dynamic dynamic); 

    }
}
