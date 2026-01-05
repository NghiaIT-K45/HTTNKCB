package com.hospitaltriage.tests;

import com.hospitaltriage.config.Config;
import com.hospitaltriage.pages.ReportsPage;
import com.hospitaltriage.utils.TestData;
import org.testng.Assert;
import org.testng.annotations.Test;

import java.time.LocalDate;

/**
 * Negative/error-handling tests for Reports.
 */
public class ReportsNegativeTests extends BaseUiTest {

    @Test
    public void reports_clearDates_thenRun_shouldShowInvalidRangeError() {
        logoutIfPossible();
        loginAs("manager");

        new ReportsPage(driver)
                .open()
                .clearFromDate()
                .clearToDate()
                .runReport();

        Assert.assertTrue(driver.getPageSource().contains("Khoảng ngày không hợp lệ")
                        || driver.getPageSource().contains("bắt buộc"),
                "Expected invalid date range / required date error");

        logoutIfPossible();
    }

    @Test
    public void reports_exportCsv_invalidRange_shouldRedirectWithError() {
        logoutIfPossible();
        loginAs("manager");

        // Make sure toDate < fromDate
        String from = TestData.date(LocalDate.now());
        String to = TestData.date(LocalDate.now().minusDays(1));

        driver.get(Config.baseUrl() + "Reports/ExportCsv?fromDate=" + from + "&toDate=" + to);

        Assert.assertTrue(driver.getCurrentUrl().contains("/Reports"),
                "Should redirect back to Reports index");
        Assert.assertTrue(driver.getPageSource().contains("Khoảng ngày không hợp lệ"),
                "Expected error flash message for invalid date range export");

        logoutIfPossible();
    }

    @Test
    public void reports_accessDenied_forNurseRole() {
        logoutIfPossible();
        loginAs("nurse");

        driver.get(Config.baseUrl() + "Reports");
        Assert.assertTrue(driver.getCurrentUrl().contains("/Home/AccessDenied"),
                "Nurse should not access Reports");
        Assert.assertTrue(driver.getPageSource().contains("Bạn không có quyền"),
                "AccessDenied message should be shown");

        driver.get(Config.baseUrl() + "Reports/ExportCsv?fromDate=2025-01-01&toDate=2025-01-02");
        Assert.assertTrue(driver.getCurrentUrl().contains("/Home/AccessDenied"),
                "Nurse should not access Reports/ExportCsv");

        logoutIfPossible();
    }
}
