package com.hospitaltriage.tests;

import com.hospitaltriage.pages.TriageListPage;
import org.testng.Assert;
import org.testng.annotations.Test;

/**
 * Extra triage list behaviors.
 */
public class TriageAdditionalTests extends BaseUiTest {

    @Test
    public void triage_filterFutureDate_shouldShowEmptyMessage() {
        logoutIfPossible();
        loginAs("nurse");

        // pick a far future date to avoid existing data
        String date = "2099-01-01";

        new TriageListPage(driver)
                .open()
                .filterDate(date);

        Assert.assertTrue(driver.getPageSource().contains("Không có lượt khám nào đang chờ phân luồng"),
                "Should show empty list message when no waiting triage visits exist for the date");

        logoutIfPossible();
    }
}
