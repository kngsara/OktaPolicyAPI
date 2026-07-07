package hr.algebra.model;

import jakarta.xml.bind.annotation.XmlElement;
import jakarta.xml.bind.annotation.XmlRootElement;

@XmlRootElement(name = "oktaPolicy")
public class OktaPolicy {

    private String oktaId;
    private String name;
    private String type;
    private String status;
    private int priority;
    private String link;
    private String createdAt;
    private String lastUpdatedAt;

    public String getOktaId() {
        return oktaId;
    }

    @XmlElement
    public void setOktaId(String oktaId) {
        this.oktaId = oktaId;
    }

    public String getName() {
        return name;
    }

    @XmlElement
    public void setName(String name) {
        this.name = name;
    }

    public String getType() {
        return type;
    }

    @XmlElement
    public void setType(String type) {
        this.type = type;
    }

    public String getStatus() {
        return status;
    }

    @XmlElement
    public void setStatus(String status) {
        this.status = status;
    }

    public int getPriority() {
        return priority;
    }

    @XmlElement
    public void setPriority(int priority) {
        this.priority = priority;
    }

    public String getLink() {
        return link;
    }

    @XmlElement
    public void setLink(String link) {
        this.link = link;
    }

    public String getCreatedAt() {
        return createdAt;
    }

    @XmlElement
    public void setCreatedAt(String createdAt) {
        this.createdAt = createdAt;
    }

    public String getLastUpdatedAt() {
        return lastUpdatedAt;
    }

    @XmlElement
    public void setLastUpdatedAt(String lastUpdatedAt) {
        this.lastUpdatedAt = lastUpdatedAt;
    }

    @Override
    public String toString() {
        return "OktaPolicy{" +
                "oktaId='" + oktaId + '\'' +
                ", name='" + name + '\'' +
                ", type='" + type + '\'' +
                ", status='" + status + '\'' +
                ", priority=" + priority +
                ", link='" + link + '\'' +
                ", createdAt='" + createdAt + '\'' +
                ", lastUpdatedAt='" + lastUpdatedAt + '\'' +
                '}';
    }
}