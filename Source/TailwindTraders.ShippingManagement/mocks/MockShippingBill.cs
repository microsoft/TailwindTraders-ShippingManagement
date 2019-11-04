using System;
using System.Collections.Generic;
using TailwindTraders.ShippingManagement.Models;

namespace TailwindTraders.ShippingManagement.Mocks
{
    public static class MockShippingBill
    {

        public static PackagingSlip GetScanResult()
        {
            var shippingBill = new PackagingSlip()
            {
                ID = new Guid().ToString(),
                Reference = "EEEEEEEE",
                Date = "02/10/2019",
                ReceivedBy = "Laura Garcia",
                Customer = "Plainconcepts",
                Amount = 25000,
                Provider = "Contoso Logistics",
                Location = "Barcelona, Spain",
                Boxes = 3,
                Items = new List<Item>() { }
            };


            var reference = new ItemProperty() { Value = "11111", Accuracy = 1 };
            var description = new ItemProperty() { Value = "Power Drill", Accuracy = 0.6 };
            var quantity = new ItemProperty() { Value = "10", Accuracy = 1 };
            var amount = new ItemProperty() { Value = "1000", Accuracy = 0.6 };

            var item = new Item()
            {
                ID = 1,
                HasPotentialErrors = true,
                Reference = reference,
                Description = description,
                Quantity = quantity,
                Amount = amount
            };

            shippingBill.Items.Add(item);


            reference = new ItemProperty() { Value = "22222", Accuracy = 1 };
            description = new ItemProperty() { Value = "Chainsaw", Accuracy = 1 };
            quantity = new ItemProperty() { Value = "5", Accuracy = 1 };
            amount = new ItemProperty() { Value = "12500", Accuracy = 1 };

            item = new Item()
            {
                ID = 2,
                HasPotentialErrors = false,
                Reference = reference,
                Description = description,
                Quantity = quantity,
                Amount = amount
            };

            shippingBill.Items.Add(item);

            reference = new ItemProperty() { Value = "33333", Accuracy = 0.6 };
            description = new ItemProperty() { Value = "Sander", Accuracy = 0.6 };
            quantity = new ItemProperty() { Value = "50", Accuracy = 0.6 };
            amount = new ItemProperty() { Value = "5000", Accuracy = 0.6 };

            item = new Item()
            {
                ID = 3,
                HasPotentialErrors = true,
                Reference = reference,
                Description = description,
                Quantity = quantity,
                Amount = amount
            };

            shippingBill.Items.Add(item);

            reference = new ItemProperty() { Value = "12547", Accuracy = 0.6 };
            description = new ItemProperty() { Value = "Grinder", Accuracy = 0.6 };
            quantity = new ItemProperty() { Value = "4", Accuracy = 0.6 };
            amount = new ItemProperty() { Value = "8000", Accuracy = 0.6 };

            item = new Item()
            {
                ID = 4,
                HasPotentialErrors = true,
                Reference = reference,
                Description = description,
                Quantity = quantity,
                Amount = amount
            };

            shippingBill.Items.Add(item);

            return shippingBill;
        }


    }
}
