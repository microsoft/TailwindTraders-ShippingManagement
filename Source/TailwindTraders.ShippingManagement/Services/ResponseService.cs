using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Azure.AI.FormRecognizer.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

using TailwindTraders.ShippingManagement.Models;
using TailwindTraders.ShippingManagement.Services.Contracts;

namespace TailwindTraders.ShippingManagement.Services
{
    public class ResponseService : IResponseService
    {
        private readonly Settings _settings;
        private WrapForm _wrap;
        private const string C_WrapFileName = "wrapmodel.json";
        private const string C_NullValue = "_null_";

        public PackagingSlip _model;

        public ResponseService(IOptions<Settings> settings)
        {
            _settings = settings.Value;
        }

        public PackagingSlip Parse(RecognizedFormCollection result)
        {
            try
            {
                if (result.Count > 0)
                {
                    _wrap = ReadJson();
                    _model = new PackagingSlip();
                    _model.ID = Guid.NewGuid().ToString();

                    ParseHeader(result.First());
                    List<Item> items = new List<Item>();

                    foreach (var page in result)
                    {
                        items.AddRange(ParseItems(page));
                    }

                    CheckItemsError(items);               //Checks errors items    
                    _model.Items = items;
                    return _model;
                }

                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public bool ChecksLocation(string requestLocation)
        {
            return _model == null || string.IsNullOrEmpty(requestLocation) || string.IsNullOrEmpty(_model.Location)
                ? false
                : requestLocation.ToLowerInvariant().Trim() == _model.Location.ToLowerInvariant().Trim();
        }

        private WrapForm ReadJson()
        {
            var home = Environment.GetEnvironmentVariable("HOME", EnvironmentVariableTarget.Process);
            var folder = home != null ? Path.Combine(home, "site", "wwwroot") : Environment.CurrentDirectory;
            var filePath = Path.Combine(folder, C_WrapFileName);
            var json = File.ReadAllText(filePath);

            return JsonConvert.DeserializeObject<WrapForm>(json);
        }

        private void ParseHeader(RecognizedForm page)
        {
            foreach (var header in _wrap.Keys)
            {
                if (page.Fields.Any(c => c.Key[header.KeyPos].ToString() == header.Key))
                {
                    var kv = page.Fields.Where(c => c.Key[header.KeyPos].ToString() == header.Key).First();

                    if (kv.Value != null)
                    {
                        switch (header.Type)
                        {
                            case "int":
                                _model.SetProperty(header.Property, int.TryParse(kv.Value.ValueData.Text, out int n) ? n : default);
                                break;
                            case "decimal":
                                _model.SetProperty(header.Property, decimal.TryParse(kv.Value.ValueData.Text, out decimal d) ? d : default);
                                break;
                            case "string":
                            default:
                                _model.SetProperty(header.Property, kv.Value.ValueData.Text);
                                break;
                        }
                    }
                }
            }
        }

        private List<Item> ParseItems(RecognizedForm page, int numTable = 0)
        {
            List<Item> items = new List<Item>();

            foreach (var t in page.Pages.First().Tables)
            {
                ProvisioningItems(t, items);

                foreach (var c in t.Cells) {
                    var index = 0;
                    Item item = items[index];
                    item.ID = index;

                        ItemProperty itemProp = new ItemProperty()
                        {
                            Value = c.Text,
                            Accuracy = String.IsNullOrEmpty(c.Confidence.ToString()) ? (double)c.Confidence : 0
                        };

                        var header = _wrap.Tables[numTable].Headers.Where(h => h.Key == c.Text).FirstOrDefault();

                        if (header != null)
                            item.SetProperty(header.Property, itemProp);


                    index++;
                  
                }
            }

            return items;
        }

        private void CheckItemsError(List<Item> items)
        {
            //checks 1. Accuracy
            var errors = items.Where(c => (c.Amount != null && c.Amount.Accuracy < _settings.FormRecognizedMinAccuracyAllowed) ||
                                          (c.Description != null && c.Description.Accuracy < _settings.FormRecognizedMinAccuracyAllowed) ||
                                          (c.Quantity != null && c.Quantity.Accuracy < _settings.FormRecognizedMinAccuracyAllowed) ||
                                          (c.Reference != null && c.Reference.Accuracy < _settings.FormRecognizedMinAccuracyAllowed)).ToList();

            errors.ForEach(e => e.HasPotentialErrors = true);

            //checks 2. Type is correct
            foreach (Item item in items)
            {
                foreach (Header header in _wrap.Tables[0].Headers)
                {
                    ItemProperty prop = item.GetProperty<ItemProperty>(header.Property);

                    if (!item.HasPotentialErrors && !string.IsNullOrEmpty(prop.Value))
                    {
                        switch (header.Type)
                        {
                            case "int":
                                int intout;
                                item.HasPotentialErrors = !int.TryParse(prop.Value, out intout);
                                prop.Accuracy = !int.TryParse(prop.Value, out intout) ? default : prop.Accuracy;

                                break;
                            case "decimal":
                                decimal decout;
                                item.HasPotentialErrors = !decimal.TryParse(prop.Value, out decout);
                                prop.Accuracy = !decimal.TryParse(prop.Value, out decout) ? default : prop.Accuracy;

                                break;
                            case "string":
                                item.HasPotentialErrors = prop.Value.ToLower().Contains(C_NullValue);
                                prop.Accuracy = prop.Value.ToLower().Contains(C_NullValue) ? default : prop.Accuracy;
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
        }
        private void ProvisioningItems(FormTable table, List<Item> items)
        {
            int maxitems = 0;
 
            if (table.Cells.Count > maxitems)
            {
                maxitems = table.Cells.Count;
            }

            for (var i = 0; i < maxitems; i++)
            {
                items.Add(new Item());
            }
        }
    }
}
