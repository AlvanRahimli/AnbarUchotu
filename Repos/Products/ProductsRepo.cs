using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AnbarUchotu.Data;
using AnbarUchotu.Models;
using AnbarUchotu.Models.Dtos;
using AnbarUchotu.Utilities;
using Microsoft.EntityFrameworkCore;

namespace AnbarUchotu.Repos.Products
{
    public class ProductsRepo : IProductsRepo
    {
        private readonly AnbarUchotuDbContext _context;

        public ProductsRepo(AnbarUchotuDbContext context)
        {
            this._context = context;
        }

        public async Task<ProductReturnDto> Product(string guid)
        {
            var product = await _context.Products
                .AsNoTracking()
                .Select(p => new ProductReturnDto()
                {
                    Guid = p.Guid,
                    Barcode = p.Barcode,
                    Count = p.Count,
                    Description = p.Description,
                    Mass = p.Mass,
                    Name = p.Name,
                    Price = p.Price
                })
                .FirstOrDefaultAsync(p => p.Guid == guid);

            return product;
        }

        public async Task<List<ProductReturnDto>> Products(int rn, int c)
        {
            var products = await _context.Products
                .AsNoTracking()
                .Skip((rn - 1) * c)
                .Take(c)
                .Select(p => new ProductReturnDto()
                {
                    Guid = p.Guid,
                    Barcode = p.Barcode,
                    Count = p.Count,
                    Description = p.Description,
                    Mass = p.Mass,
                    Name = p.Name,
                    Price = p.Price
                })
                .ToListAsync();

            return products;
        }

        public async Task<ProductReturnDto> Register(ProductRegisterDto product)
        {
            var newProduct = new Product()
            {
                Guid = Guid.NewGuid().ToString(),
                Barcode = product.Barcode,
                Name = product.Name,
                Description = product.Description,
                Count = product.Count,
                Mass = product.Mass,
                Price = product.Price
            };

            await _context.Products.AddAsync(newProduct);
            var result = await _context.SaveChangesAsync();
            if (result > 0)
            {
                var returnValue = await Product(newProduct.Guid);
                return returnValue;
            }
            return null;
        }

        public async Task<ProductReturnDto> Update(ProductUpdateDto product)
        {
            var oldP = await _context
                .Products.FirstOrDefaultAsync(p => p.Guid == product.Guid);

            if (oldP != null)
            {
                oldP.Name = product.Name;
                oldP.Description = product.Description;
                oldP.Barcode = product.Barcode;
                oldP.Count = product.Count;
                oldP.Mass = product.Mass;
                oldP.Price = product.Price;
            }

            await _context.SaveChangesAsync();

            var p = await Product(product.Guid);
            return p;
        }

        public async Task<bool> Delete(string guid)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Guid == guid);

            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<ProductReturnDto> AddExisting(string barcode, int count)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Barcode == barcode);

            if (product != null)
            {
                product.Count += count;
                int res = await _context.SaveChangesAsync();
                if (res > 0)
                {
                    var returnProduct = await _context.Products
                        .AsNoTracking()
                        .Select(p => new ProductReturnDto()
                        {
                            Guid = p.Guid,
                            Barcode = p.Barcode,
                            Description = p.Description,
                            Count = p.Count,
                            Mass = p.Mass,
                            Name = p.Name,
                            Price = p.Price
                        })
                        .FirstOrDefaultAsync(p => p.Guid == product.Guid);

                    return returnProduct;
                }
            }
            return null;
        }
    }
}