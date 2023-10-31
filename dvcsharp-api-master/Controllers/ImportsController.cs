using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using dvcsharp_core_api.Models;
using dvcsharp_core_api.Data;

namespace dvcsharp_core_api
{
    [Route("api/[controller]")]
    public class ImportsController : Controller
    {
        private readonly GenericDataContext _context;

        public ImportsController(GenericDataContext context)
        {
            _context = context;
        }


        [HttpPost]
        public IActionResult Post()
        {
            try
            {
                var entities = new List<object>();

                using (var reader = new StreamReader(HttpContext.Request.Body))
                {
                    var jsonString = reader.ReadToEnd();

                    // Validar que el contenido JSON sea seguro antes de la deserialización
                    if (EsContenidoJSONSeguro(jsonString))
                    {
                        var jsonObject = JsonConvert.DeserializeObject<JObject>(jsonString);

                        if (jsonObject["Entities"] is JArray entitiesArray)
                        {
                            foreach (var entityToken in entitiesArray)
                            {
                                // Validar que solo se permitan tipos específicos
                                string typeName = entityToken["Type"]?.Value<string>();
                                if (!string.IsNullOrEmpty(typeName) && EsTipoPermitido(typeName))
                                {
                                    entities.Add(entityToken.ToObject(Type.GetType(typeName)));
                                }
                            }
                        }
                    }
                    else
                    {
                        // Manejar contenido JSON no seguro
                        return BadRequest("Contenido JSON no seguro");
                    }
                }

                // Realizar acciones seguras con los datos deserializados (si es necesario)
                // ...

                return Ok(entities);
            }
            catch (Exception ex)
            {
                // Manejo de excepciones (registra, notifica, etc.)
                Console.WriteLine("Error durante la deserialización: " + ex.Message);
                return BadRequest("Error durante la deserialización");
            }
        }

        private bool EsContenidoJSONSeguro(string jsonContent)
        {
            // Implementa lógica para validar que el contenido JSON sea seguro
            // Retorna true si es seguro, de lo contrario, false.
            // Puedes agregar reglas específicas de validación según tus necesidades.

            // Ejemplo simplificado: Solo permite JSON que contiene ciertos elementos esperados
            return jsonContent.Contains("\"Entities\"") && jsonContent.Contains("\"Type\"");
        }

        private bool EsTipoPermitido(string typeName)
        {
            // Implementa lógica para validar que el tipo sea permitido
            // Retorna true si es permitido, de lo contrario, false.
            // Puedes mantener una lista blanca de tipos permitidos, por ejemplo.
            // Ejemplo simplificado:
            var tiposPermitidos = new List<string> { "Tipo1", "Tipo2" }; // Ajusta según tus necesidades
            return tiposPermitidos.Contains(typeName);
        }
    }
}