using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSP.Shared.Enums
{
    public enum TodoStatus
    {
        // Todo vừa được AI generated ra từ meeting
        Generated = 0,

        // PM đang review/edit todo
        UnderReview = 1,

        // Todo đã được convert thành Task chính thức
        ConvertedToTask = 2,

        // User quyết định xóa todo (soft delete)
        Deleted = 3
    }
}
