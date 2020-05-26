using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BaXter;
using System.IO;
using System.Collections.Generic;

namespace BaXter_Usage_Samples
{
    [TestClass]
    public class BaXterSamples
    {
        static readonly string eval_license = "Please visit https://developer.wacom.com/developer-dashboard to generate an Evaluation License for the 'WILL for Documents' SDK";
        static readonly string test_document = "..\\..\\..\\CLB Paper Sample.pdf";
        static readonly string test_document_edited = "..\\..\\..\\CLB Paper Sample_edited.pdf";

        static readonly string blank_doc = "..\\..\\..\\Blank.pdf";
        static readonly string blank_doc_with_barcode = "..\\..\\..\\Blank_barcode.pdf";

        static readonly string blank_form = "..\\..\\..\\Blank_form.pdf";
        static readonly string blank_form_prepared = "..\\..\\..\\Blank_form_prepared.pdf";

        [TestMethod]
        public void EditingAnExistingIDMFDocument()
        {
            bool exception_thrown = false;
            try
            {
                // Before using any of the BaXter IDML, be sure to set your license key.
                Utility.setLicense(eval_license);

                // Before we make any changes, going to make a copy of the provided
                // sample document.
                if (File.Exists(test_document_edited)) File.Delete(test_document_edited);
                File.Copy(test_document, test_document_edited);

                // To edit an existing PDF with IDMF metadata in, open it via
                // a BaXter::Reader
                var reader = new Reader();
                reader.readFromFile(test_document_edited);

                // Now Reader has parsed the file, we can read or edit as we please.
                reader.document.authoringToolName = "BaXter Sample Tool";
                reader.document.smartPadDeviceName = "My PHU-111 2018";
                reader.document.clientAppName = "BaXter Sample Client";

                // To add a barcode, we need to set a Page's UUID & UUID_TYPE
                reader.document.pages[0].uuid = "12345678";
                reader.document.pages[0].uuid_type = BarcodeSymbology.Code128;
                // Then add the barcode!
                reader.document.pages[0].addBarcode(test_document_edited);

                // Lastly we can edit some field level data
                reader.document.pages[0].fields[0].data = "New Data!";

                // As we've made many metadata changes, including a barcode we 
                // must commit those changes to the PDF.
                reader.finaliseMetadata();
            }
            catch (Exception ex)
            {
                exception_thrown = true;
                Console.WriteLine(ex.Message);
            }
            Assert.IsFalse(exception_thrown);
        }

        [TestMethod]
        public void AddBarcodeToEmptyDocument()
        {
            bool exception_thrown = false;
            try
            {
                Utility.setLicense(eval_license);
                // We're going to use a blank document for this test.
                // Before we make any changes, going to make a copy of the provided
                // sample document.
                if (File.Exists(blank_doc_with_barcode)) File.Delete(blank_doc_with_barcode);
                File.Copy(blank_doc, blank_doc_with_barcode);

                // Firstly we need the bare minimum of useful Document level Metadata
                Document document = new Document();
                document.authoringToolName = "BaXter Sample Code";
                document.authoringToolVersion = "Test";

                // Barcodes are ofcourse tied to Pages, so we need a new Page level metadata
                Page page_1 = new Page();
                page_1.pdfPage = "1";
                // To add a barcode, we need to set a Page's UUID & UUID_TYPE
                page_1.uuid = "12345678";
                page_1.uuid_type = BarcodeSymbology.Code128;
                // Then add the barcode!
                page_1.addBarcode(blank_doc_with_barcode);

                // Finally add the Page to the Document
                var page_id_list = new System.Collections.Generic.List<Tuple<string, string>>();
                page_id_list.Add(new Tuple<string, string>(page_1.pdfPage, page_1.uuid));
                document.pageIDList = page_id_list;

                document.pageCompletionOrder = new System.Collections.Generic.List<int> { 1 };

                document.addPage(page_1);

                // Lastly to commit all the above metadata to file, we should finalise.
                document.finalise(blank_doc_with_barcode);
                // Make sure to finalise the pages aswell, to commit the barcode
                foreach (var page in document.pages)
                {
                    page.finalise(blank_doc_with_barcode);
                }
            }
            catch (Exception ex)
            {
                exception_thrown = true;
                Console.WriteLine(ex.Message);
            }
            Assert.IsFalse(exception_thrown);
        }

        [TestMethod]
        public void CreateNewIDMFDocument()
        {
            /// This example uses a pre-prepared mostly blank PDF with 2 AcroForms
            /// The first is a text box, and the second a check box.
            /// We will add a barcode and define those form fields for later
            /// client data capture.
            bool exception_thrown = false;
            try
            {
                Utility.setLicense(eval_license);
                // We're going to use a blank document for this test.
                // Before we make any changes, going to make a copy of the provided
                // sample document.
                if (File.Exists(blank_form_prepared)) File.Delete(blank_form_prepared);
                File.Copy(blank_form, blank_form_prepared);

                // Firstly we need the bare minimum of useful Document level Metadata
                Document document = new Document();
                document.authoringToolName = "BaXter Sample Code";
                document.authoringToolVersion = "Test";
                document.pageCompletionOrder.Add(1);

                // Barcodes are ofcourse tied to Pages, so we need a new Page level metadata
                Page page_1 = new Page();
                page_1.pdfPage = "1";
                // To add a barcode, we need to set a Page's UUID & UUID_TYPE
                page_1.uuid = "123456789012";
                page_1.uuid_type = BarcodeSymbology.EAN;
                // Then add the barcode!
                page_1.addBarcode(blank_form_prepared);

                // Creating the first Text field
                var text_field = new Field();
                text_field.locationH = "22";
                text_field.locationW = "150";
                // The location origin in this case is bottom left
                text_field.locationX = "48";
                text_field.locationY = "742";
                text_field.pdfID = "Text Field"; // This MUST match the name of the underlying PDF Form Object
                text_field.required = true;
                text_field.type = "Text";
                text_field.UUID = "001";
                text_field.tag = "My Text Field";

                // Add it to the page
                page_1.addField(text_field);

                // Creating the Check Box Field
                var checkbox_field = new Field();
                checkbox_field.locationH = "30";
                checkbox_field.locationW = "30";
                // The location origin in this case is bottom left
                checkbox_field.locationX = "262";
                checkbox_field.locationY = "740";
                checkbox_field.pdfID = "Check Box"; // This MUST match the name of the underlying PDF Form Object
                checkbox_field.required = true;
                checkbox_field.type = "Boolean";
                checkbox_field.UUID = "002";
                checkbox_field.tag = "My Check Box Field";

                // Add it to the page
                page_1.addField(checkbox_field);

                var field_id_list = new List<string> { text_field.pdfID, checkbox_field.pdfID };
                page_1.fieldIDList = field_id_list;

                // Finally add the Page to the Document
                var page_id_list = new List<Tuple<string, string>>();
                page_id_list.Add(new Tuple<string, string>(page_1.pdfPage, page_1.uuid));
                document.pageIDList = page_id_list;

                document.pageCompletionOrder = new List<int> { 1 };

                document.addPage(page_1);

                // Lastly to commit all the above metadata to file, we should finalise the document.
                document.finalise(blank_form_prepared);
                // Make sure to finalise the pages & fields aswell.
                foreach (var page in document.pages)
                {
                    foreach (var field in page.fields)
                    {
                        field.finalise(blank_form_prepared, 0);
                    }
                    page.finalise(blank_form_prepared);
                }
            }
            catch (Exception ex)
            {
                exception_thrown = true;
                Console.WriteLine(ex.Message);
            }
            Assert.IsFalse(exception_thrown);
        }
    }
}

