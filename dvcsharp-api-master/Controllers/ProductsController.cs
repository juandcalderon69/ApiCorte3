using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using dvcsharp_core_api.Models;
using dvcsharp_core_api.Data;

namespace dvcsharp_core_api
{
    [Route("api/[controller]")]
    public class ProductsController : Controller
    {
        private readonly GenericDataContext _context;

        public ProductsController(GenericDataContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IEnumerable<Product> Get()
        {
            return _context.Products.ToList();
        }

        [HttpPost]
        public IActionResult Post([FromBody] Product product)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingProduct = _context.Products
                .FirstOrDefault(p => p.name == product.name || p.skuId == product.skuId);

            if (existingProduct != null)
            {
                ModelState.AddModelError("name", "Product name or skuId is already taken");
                return BadRequest(ModelState);
            }

            _context.Products.Add(product);
            _context.SaveChanges();

            return Ok(product);
        }

        [HttpGet("export")]
        public void Export()
        {
            XmlRootAttribute root = new XmlRootAttribute("Entities");
            XmlSerializer serializer = new XmlSerializer(typeof(Product[]), root);

            Response.ContentType = "application/xml";
            serializer.Serialize(HttpContext.Response.Body, _context.Products.ToArray());
        }

        [HttpGet("search")]
        public IActionResult Search(string keyword)
        {
            if (String.IsNullOrEmpty(keyword))
            {
                return Ok("Cannot search without a keyword");
            }

            // Utilizar Entity Framework Core para generar consultas seguras y parametrizadas
            var products = _context.Products
                .Where(p => EF.Functions.Like(p.name, $"%{keyword}%") || EF.Functions.Like(p.description, $"%{keyword}%"))
                .ToList();

            return Ok(products);
        }
[HttpPost("import")]
public IActionResult Import()
{
    try
    {
        using (StreamReader reader = new StreamReader(HttpContext.Request.Body))
        {
            string requestBody = reader.ReadToEnd();

            // Validar el XML contra un esquema
            XmlSchemaSet schemas = new XmlSchemaSet();
            schemas.Add("", "ruta/al/esquema.xsd"); // Reemplaza con la ruta de tu esquema XML

            XmlReaderSettings settings = new XmlReaderSettings();
            settings.Schemas = schemas;
            settings.ValidationType = ValidationType.Schema;

            XmlSerializer serializer = new XmlSerializer(typeof(Product[]), new XmlRootAttribute("Entities"));

            using (var stringReader = new StringReader(requestBody))
            using (var validatingReader = XmlReader.Create(stringReader, settings))
            {
                var entities = (Product[])serializer.Deserialize(validatingReader);

                // Validar y procesar los datos deserializados según sea necesario
                foreach (var product in entities)
                {
                    // Realizar acciones seguras con los datos deserializados
                    // Por ejemplo, puedes agregar la validación de datos aquí
                }

                return Ok(entities);
            }
        }
    }
    catch (Exception ex)
    {
        return BadRequest($"Error during XML deserialization: {ex.Message}");
    }
}

    }
}
