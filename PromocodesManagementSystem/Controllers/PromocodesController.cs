using System;
using System.Collections.Generic;
using System.Drawing;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IronBarCode;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PromocodesManagementSystem.Data;
using PromocodesManagementSystem.Model;

namespace PromocodesManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PromocodesController : ControllerBase
    {
        private readonly PromocodesContext _context;
        private static IConfiguration _configuration;
        public PromocodesController(PromocodesContext context,IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;     
        }

        [AllowAnonymous]
        [HttpPost("GetToken")]
        public async Task<IActionResult> GetToken(string userName, string password)
        {
            var tokenUser = (from o in _context.TblTokenusers
                             where
                             o.UserName == userName && o.Password == password
                             select o).FirstOrDefault();
            if (tokenUser != null)
            {
                var issuer = _configuration.GetSection("Jwt").GetValue<string>("Issuer");
                var audience = _configuration.GetSection("Jwt").GetValue<string>("Audience");
                var key = Encoding.ASCII.GetBytes
                (_configuration.GetSection("Jwt").GetValue<string>("Key"));
                var tokenDescriptor = new SecurityTokenDescriptor
                {

                    Expires = DateTime.UtcNow.AddDays(_configuration.GetSection("Jwt").GetValue<int>("ExpireDay")),
                    Issuer = issuer,
                    Audience = audience,
                    SigningCredentials = new SigningCredentials
                    (new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha512Signature)
                };
                var tokenHandler = new JwtSecurityTokenHandler();
                var token = tokenHandler.CreateToken(tokenDescriptor);
                var jwtToken = tokenHandler.WriteToken(token);
                var stringToken = tokenHandler.WriteToken(token);
                return CreatedAtAction("GetToken", new { }, stringToken);
            }
            return NoContent();
        }

        [Authorize]
        [HttpGet("GetPromoCode")]
        public async Task<ActionResult> GetPromoCode(PromoCodeGetModel promoCodeGet)
        {
            //get unused promocodes for phone number
            var avaliablePromos = (from o in _context.TblPromocodes
                                   where o.Phone == promoCodeGet.Phone && o.IsUsed == false && o.Active == true && o.IsUsed == false
                                   select o).ToList();

            if (avaliablePromos == null)
            {
                return NotFound();
            }
            List<AvalialbePromoModel> promoResult = new List<AvalialbePromoModel>();
            foreach(var promo in avaliablePromos)
            {
                promo.IsUsed = true;
                promo.ModifiedDate = DateTime.Now;
                _context.Entry(promo).State = EntityState.Modified;
                promoResult.Add(new AvalialbePromoModel() {Code = promo.PromoCodes,QRImage = promo.Qrimage });
            }
            await _context.SaveChangesAsync();
            return CreatedAtAction("GetPromoCode", new { phone = promoCodeGet.Phone }, promoResult);
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<TblPromocode>> GeneratePromoCodes(GeneratePromoModel generatePromo)
        {
            //get counts of purchases that are already given promos
            int givenPromos = (from o in _context.TblPromocodes
                                     where o.Phone == generatePromo.Phone && o.Active == true
                                     select o).Count();
            // calculate promo codes to give based on purchased history
            int promoChance = _configuration.GetSection("PromoSetting").GetValue<int>("PromoChance");
            var PromoAavaliable = Math.DivRem((generatePromo.PurchaseCount - (givenPromos * promoChance)), promoChance, out int remainder);
            try
            {
                string promoCode = "";
                bool isValid = false;
                for (int i = 0; i < PromoAavaliable; i++)
                {                    
                    do
                    {
                        isValid = false;
                        promoCode = GenerateCodes();
                        var promoGenerated = (from o in _context.TblPromocodes where
                                              o.PromoCodes == promoCode select o).FirstOrDefault();
                        if (promoGenerated == null)
                        {
                            isValid = true;
                        }
                    }
                    while (isValid==false);
                   
                    var qrImage = GenerateQR(promoCode);
                    TblPromocode tblPromocode = new TblPromocode()
                    {
                        Id = Guid.NewGuid().ToString(),
                        PromoCodes = promoCode,
                        Qrimage = qrImage,
                        Phone = generatePromo.Phone,
                        IsUsed = false,
                        Active = true,
                        CreatedDate = DateTime.Now,
                        Amount = _configuration.GetSection("PromoSetting").GetValue<double>("PromoAmount")
                };
                    _context.TblPromocodes.Add(tblPromocode);
                }
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return Problem("Error occured " + ex.Message);
            }

            return NoContent();
        }
        static Random random = new Random();
        public static string GenerateCodes()
        {

            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string nums = "0987654321";
            string alpha = new string(Enumerable.Repeat(chars, 5)
                .Select(s => s[random.Next(s.Length)]).ToArray());
            string digit = new string(Enumerable.Repeat(nums, 6)
                .Select(s => s[random.Next(s.Length)]).ToArray());
            return digit + alpha;

        }
        public static byte[] GenerateQR(string text)
        {
            try
            {
                GeneratedBarcode barcode = QRCodeWriter.CreateQrCode(text, 200);
                barcode.AddBarcodeValueTextBelowBarcode();
                barcode.SetMargins(10);
                barcode.ChangeBarCodeColor(Color.BlueViolet);
                var bitMap = barcode.ToBitmap();
                System.IO.MemoryStream stream = new System.IO.MemoryStream();
                bitMap.Save(stream, System.Drawing.Imaging.ImageFormat.Bmp);
                byte[] imageBytes = stream.ToArray();
                return imageBytes;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
