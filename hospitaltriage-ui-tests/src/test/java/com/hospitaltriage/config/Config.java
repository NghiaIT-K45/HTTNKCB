package com.hospitaltriage.config;

import java.io.IOException;
import java.io.InputStream;
import java.util.Objects;
import java.util.Properties;

/**
 * Small config helper.
 *
 * Priority:
 *  1) System properties (-DbaseUrl, -Dbrowser, ...)
 *  2) src/test/resources/config.properties
 */
public final class Config {
    private static final Properties PROPS = new Properties();

    static {
        try (InputStream is = Config.class.getClassLoader().getResourceAsStream("config.properties")) {
            if (is != null) {
                PROPS.load(is);
            }
        } catch (IOException e) {
            throw new RuntimeException("Cannot load config.properties", e);
        }
    }

    private Config() {
    }

    public static String get(String key) {
        String sys = System.getProperty(key);
        if (sys != null && !sys.isBlank()) return sys.trim();

        String val = PROPS.getProperty(key);
        if (val == null) {
            throw new IllegalArgumentException("Missing config key: " + key);
        }
        return val.trim();
    }

    public static String getOrDefault(String key, String defaultValue) {
        String sys = System.getProperty(key);
        if (sys != null && !sys.isBlank()) return sys.trim();

        String val = PROPS.getProperty(key);
        return val == null ? defaultValue : val.trim();
    }

    public static boolean getBool(String key) {
        return Boolean.parseBoolean(getOrDefault(key, "false"));
    }

    public static String baseUrl() {
        String url = get("baseUrl");
        // normalize
        if (!url.endsWith("/")) url = url + "/";
        return url;
    }

    public static String browser() {
        return getOrDefault("browser", "chrome");
    }

    public static boolean headless() {
        return Boolean.parseBoolean(getOrDefault("headless", "false"));
    }

    public static String credential(String roleKey, String field) {
        return get(roleKey + "." + field);
    }

    public static String requireNonBlank(String value, String message) {
        if (value == null || value.isBlank()) throw new IllegalArgumentException(message);
        return value;
    }
}
