package com.hospitaltriage.utils;

import java.time.LocalDate;
import java.time.format.DateTimeFormatter;
import java.util.concurrent.ThreadLocalRandom;

public final class TestData {
    private TestData() {}

    public static String uniqueCode(String prefix) {
        // Many entity Codes in the app are max length 20.
        // Keep generated codes short, deterministic-ish, and unique.
        String ts = DateTimeFormatter.ofPattern("yyMMddHHmmss").format(java.time.LocalDateTime.now()); // 12
        int rnd = ThreadLocalRandom.current().nextInt(10, 99); // 2 digits

        String p = prefix == null ? "" : prefix;
        int maxPrefix = 20 - (ts.length() + 2);
        if (maxPrefix < 0) maxPrefix = 0;
        if (p.length() > maxPrefix) p = p.substring(0, maxPrefix);

        return p + ts + rnd;
    }

    public static String uniqueName(String prefix) {
        return prefix + " " + uniqueCode("T");
    }

    /**
     * yyyy-MM-dd for HTML <input type="date">.
     */
    public static String date(LocalDate d) {
        return d.format(DateTimeFormatter.ISO_LOCAL_DATE);
    }

    public static String vietnamesePatientName() {
        // just a deterministic-ish friendly name
        return "BN Test " + uniqueCode("_");
    }

    public static String identityNumber() {
        // not strict CCCD format, but unique and within 50 chars
        return "ID" + uniqueCode("");
    }

    public static String phone() {
        // basic VN-like 10 digits
        int tail = ThreadLocalRandom.current().nextInt(1000000, 9999999);
        return "09" + tail;
    }
}
