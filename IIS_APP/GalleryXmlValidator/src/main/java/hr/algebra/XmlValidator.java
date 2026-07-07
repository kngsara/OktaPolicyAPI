package hr.algebra;

import hr.algebra.model.OktaPolicies;
import hr.algebra.model.OktaPolicy;
import jakarta.xml.bind.JAXBContext;
import jakarta.xml.bind.Unmarshaller;

import javax.xml.XMLConstants;
import javax.xml.validation.Schema;
import javax.xml.validation.SchemaFactory;
import java.io.File;

public class XmlValidator {

    public static void main(String[] args) {
        try {
            String xmlPath = "C:\\IISProject\\shared\\okta-policies.xml";
            String xsdPath = "src/main/resources/okta-policy.xsd";

            File xmlFile = new File(xmlPath);
            File xsdFile = new File(xsdPath);

            if (!xmlFile.exists()) {
                System.out.println("XML datoteka nije pronađena: " + xmlFile.getAbsolutePath());
                System.exit(1);
            }

            if (!xsdFile.exists()) {
                System.out.println("XSD datoteka nije pronađena: " + xsdFile.getAbsolutePath());
                System.exit(1);
            }

            SchemaFactory schemaFactory = SchemaFactory.newInstance(XMLConstants.W3C_XML_SCHEMA_NS_URI);
            Schema schema = schemaFactory.newSchema(xsdFile);

            JAXBContext context = JAXBContext.newInstance(OktaPolicies.class);
            Unmarshaller unmarshaller = context.createUnmarshaller();
            unmarshaller.setSchema(schema);

            OktaPolicies policies = (OktaPolicies) unmarshaller.unmarshal(xmlFile);

            System.out.println("XML je valjan prema XSD shemi.");
            System.out.println();

            for (OktaPolicy policy : policies.getOktaPolicies()) {
                System.out.println(policy);
            }

            System.exit(0);
        } catch (Exception ex) {
            System.out.println("XML nije valjan prema XSD shemi.");
            System.out.println(ex.getMessage());
            System.exit(1);
        }
    }
}