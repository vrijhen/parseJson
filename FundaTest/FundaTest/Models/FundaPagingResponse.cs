using FundaTest.Models;
using System;
namespace FundaTest
{
    [Serializable()]
    public class FundaPagingResponse
    {
        public FundaPagingResult Paging;
        public int TotaalAantalObjecten;
    }
}
