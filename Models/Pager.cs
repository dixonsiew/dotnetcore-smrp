namespace smrp.Models
{
    public class Pager
    {
        public int Total { get; set; }
        public int PageNum { get; set; }
        public int PageSize { get; set; }

        public Pager(int total, int pageNum, int pageSize)
        {
            Total = total;
            PageNum = pageNum;
            SetPageSize(pageSize);
        }

        public void SetPageSize(int pageSize)
        {
            if ((Total < pageSize || pageSize < 1) && Total > 0)
            {
                PageSize = Total;
            }

            else
            {
                PageSize = pageSize;
            }
        }

        public int LowerBound
        {
            get
            {
                return (PageNum - 1) * PageSize;
            }
        }

        public int UpperBound
        {
            get
            {
                var x = PageNum * PageSize;
                if (Total < x)
                {
                    x = Total;
                }

                return x;
            }
        }

        public int TotalPages
        {
            get
            {
                var v = Total * 1.00 / PageSize;
                var x = Math.Ceiling(v);
                return (int)x;
            }
        }
    }
}
