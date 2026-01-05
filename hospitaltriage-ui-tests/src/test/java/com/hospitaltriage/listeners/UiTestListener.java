package com.hospitaltriage.listeners;

import com.hospitaltriage.tests.BaseUiTest;
import org.openqa.selenium.OutputType;
import org.openqa.selenium.TakesScreenshot;
import org.openqa.selenium.WebDriver;
import org.testng.ITestContext;
import org.testng.ITestListener;
import org.testng.ITestResult;
import org.testng.Reporter;

import java.io.PrintWriter;
import java.io.StringWriter;
import java.nio.charset.StandardCharsets;
import java.nio.file.Files;
import java.nio.file.Path;
import java.nio.file.StandardOpenOption;
import java.time.LocalDateTime;
import java.time.format.DateTimeFormatter;

/**
 * TestNG listener:
 *  - writes a readable execution log to target/test-output/execution.log
 *  - on failure: captures screenshot + page source to target/test-output
 *
 * This helps you quickly inspect failures after running: mvn clean test
 */
public final class UiTestListener implements ITestListener {

    private static final DateTimeFormatter TS = DateTimeFormatter.ofPattern("yyyyMMdd_HHmmss_SSS");
    private static final Object LOCK = new Object();

    private static Path outDir() {
        return Path.of("target", "test-output");
    }

    private static Path logFile() {
        return outDir().resolve("execution.log");
    }

    private static void ensureDirs() {
        try {
            Files.createDirectories(outDir());
            Files.createDirectories(outDir().resolve("screenshots"));
            Files.createDirectories(outDir().resolve("pagesource"));
        } catch (Exception e) {
            // best-effort: do not fail test execution because of logging
            System.err.println("[UiTestListener] Cannot create output folders: " + e.getMessage());
        }
    }

    private static void logLine(String line) {
        ensureDirs();
        String msg = "[" + LocalDateTime.now().format(TS) + "] " + line + System.lineSeparator();
        synchronized (LOCK) {
            try {
                Files.writeString(
                        logFile(),
                        msg,
                        StandardCharsets.UTF_8,
                        StandardOpenOption.CREATE,
                        StandardOpenOption.APPEND
                );
            } catch (Exception e) {
                // best-effort
                System.err.println("[UiTestListener] Cannot write log: " + e.getMessage());
            }
        }
    }

    private static String safeName(String raw) {
        return raw
                .replaceAll("[^a-zA-Z0-9._-]", "_")
                .replaceAll("_+", "_");
    }

    private static WebDriver extractDriver(ITestResult result) {
        try {
            Object instance = result.getInstance();
            if (instance instanceof BaseUiTest t) {
                return t.getDriver();
            }
        } catch (Exception ignored) {
        }
        return null;
    }

    private static String testId(ITestResult result) {
        String cls = result.getTestClass() == null ? "" : result.getTestClass().getName();
        String m = result.getMethod() == null ? "" : result.getMethod().getMethodName();
        return safeName(cls + "." + m);
    }

    @Override
    public void onStart(ITestContext context) {
        logLine("=== SUITE START: " + context.getName() + " ===");
    }

    @Override
    public void onFinish(ITestContext context) {
        logLine("=== SUITE FINISH: " + context.getName() + " ===");
    }

    @Override
    public void onTestStart(ITestResult result) {
        logLine("START  " + testId(result));
    }

    @Override
    public void onTestSuccess(ITestResult result) {
        logLine("PASS   " + testId(result));
    }

    @Override
    public void onTestSkipped(ITestResult result) {
        logLine("SKIP   " + testId(result) + " | " + (result.getThrowable() == null ? "" : result.getThrowable().getMessage()));
    }

    @Override
    public void onTestFailure(ITestResult result) {
        WebDriver driver = extractDriver(result);
        String id = testId(result);
        String ts = LocalDateTime.now().format(TS);

        String currentUrl = "";
        if (driver != null) {
            try {
                currentUrl = driver.getCurrentUrl();
            } catch (Exception ignored) {
            }
        }

        logLine("FAIL   " + id + (currentUrl.isBlank() ? "" : (" | URL=" + currentUrl)));

        // write stack trace
        if (result.getThrowable() != null) {
            StringWriter sw = new StringWriter();
            result.getThrowable().printStackTrace(new PrintWriter(sw));
            logLine("ERROR  " + id + "\n" + sw);
        }

        if (driver == null) {
            Reporter.log("[UiTestListener] WebDriver is null; cannot capture artifacts.", true);
            return;
        }

        // Screenshot
        try {
            if (driver instanceof TakesScreenshot tsDriver) {
                byte[] png = tsDriver.getScreenshotAs(OutputType.BYTES);
                Path p = outDir().resolve("screenshots").resolve(id + "_" + ts + ".png");
                Files.write(p, png);
                Reporter.log("Screenshot: " + p.toString(), true);
                logLine("ARTIFACT " + id + " | screenshot=" + p);
            }
        } catch (Exception e) {
            logLine("ARTIFACT " + id + " | screenshot FAILED: " + e.getMessage());
        }

        // Page source
        try {
            String html = driver.getPageSource();
            Path p = outDir().resolve("pagesource").resolve(id + "_" + ts + ".html");
            Files.writeString(p, html, StandardCharsets.UTF_8);
            Reporter.log("PageSource: " + p.toString(), true);
            logLine("ARTIFACT " + id + " | pagesource=" + p);
        } catch (Exception e) {
            logLine("ARTIFACT " + id + " | pagesource FAILED: " + e.getMessage());
        }
    }
}
