package hr.algebra.model;

import jakarta.xml.bind.annotation.XmlElement;
import jakarta.xml.bind.annotation.XmlRootElement;
import java.util.ArrayList;
import java.util.List;

@XmlRootElement(name = "oktaPolicies")
public class OktaPolicies {

    private List<OktaPolicy> oktaPolicies = new ArrayList<>();

    public List<OktaPolicy> getOktaPolicies() {
        return oktaPolicies;
    }

    @XmlElement(name = "oktaPolicy")
    public void setOktaPolicies(List<OktaPolicy> oktaPolicies) {
        this.oktaPolicies = oktaPolicies;
    }
}