using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using TobinTaxer.OptionModels;

namespace TobinTaxer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaxRateController : ControllerBase
    {
        private readonly TaxInfo _taxInfo;

        public TaxRateController(IOptionsMonitor<TaxInfo> taxOptions)
        {
            _taxInfo = taxOptions.CurrentValue;
        }

        [HttpGet]
        public ActionResult<double> Get()
        {
            return Ok(_taxInfo.TaxRate);
        }
    }
}