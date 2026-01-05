package com.hospitaltriage.tests;

import com.hospitaltriage.pages.*;
import com.hospitaltriage.utils.TestData;
import org.testng.Assert;
import org.testng.annotations.Test;

import java.time.LocalDate;

public class DashboardAndReportsTests extends BaseUiTest {

    private void createAndTriageVisit(String patientName,
                                      String identityNumber,
                                      String symptoms,
                                      String deptOverrideVisibleText,
                                      String doctorOverrideVisibleText) {

        // Create visit as Receptionist
        logoutIfPossible();
        loginAs("receptionist");

        String dob = TestData.date(LocalDate.now().minusYears(30));

        new IntakePage(driver).open()
                .setFullName(patientName)
                .setDateOfBirth(dob)
                .selectGender("Nam")
                .setIdentityNumber(identityNumber)
                .setPhone(TestData.phone())
                .setInsuranceCode("BHYT" + identityNumber)
                .setAddress("TP.HCM")
                .submit();

        // Triage as Nurse
        logoutIfPossible();
        loginAs("nurse");

        String today = TestData.date(LocalDate.now());
        TriageProcessPage process = new TriageListPage(driver)
                .open()
                .filterDate(today)
                .clickProcessByPatientName(patientName);

        process.setSymptoms(symptoms);
        if (deptOverrideVisibleText != null) process.selectDepartment(deptOverrideVisibleText);
        if (doctorOverrideVisibleText != null) process.selectDoctor(doctorOverrideVisibleText);
        process.submit();
    }

    @Test
    public void dashboard_admin_canSelectDepartment_andFilterByDepartment() {
        logoutIfPossible();
        loginAs("admin");

        DashboardPage dash = new DashboardPage(driver).open();
        Assert.assertTrue(dash.canSelectDepartment(), "Admin should be able to select department on Dashboard");

        dash.filterDepartment("NOI - Khoa Nội");
        Assert.assertTrue(driver.getCurrentUrl().contains("departmentId="),
                "After filtering, URL should contain departmentId param");

        logoutIfPossible();
    }

    @Test
    public void dashboard_doctor_isRestrictedToOwnDepartment() {
        String patientNoi = "BN NOI " + TestData.uniqueCode("");
        String patientNgoai = "BN NGOAI " + TestData.uniqueCode("");

        // Rule keywords seeded:
        // - "ho" -> Khoa Nội
        // - "chấn thương" -> Khoa Ngoại
        createAndTriageVisit(patientNoi, TestData.identityNumber(), "ho nhẹ", null, null);
        createAndTriageVisit(patientNgoai, TestData.identityNumber(), "chấn thương nhẹ", null, null);

        logoutIfPossible();
        loginAs("doctor1");

        DashboardPage dash = new DashboardPage(driver).open();
        Assert.assertFalse(dash.canSelectDepartment(),
                "Doctor should NOT be able to choose department (forced to own dept)");

        Assert.assertTrue(dash.hasPatient(patientNoi), "Doctor DR001 should see visits of Khoa Nội");
        Assert.assertFalse(dash.hasPatient(patientNgoai), "Doctor DR001 should NOT see visits of other departments");

        logoutIfPossible();
    }

    @Test
    public void reports_manager_canRunReport_andSeeExportLink() {
        // Ensure there is at least 1 visit today so the table is not empty.
        String patientName = "BN Report " + TestData.uniqueCode("");
        String idNo = TestData.identityNumber();

        logoutIfPossible();
        loginAs("receptionist");
        new IntakePage(driver).open()
                .setFullName(patientName)
                .setDateOfBirth(TestData.date(LocalDate.now().minusYears(40)))
                .selectGender("Nam")
                .setIdentityNumber(idNo)
                .setPhone(TestData.phone())
                .setAddress("HN")
                .submit();

        logoutIfPossible();
        loginAs("manager");

        String today = TestData.date(LocalDate.now());

        ReportsPage reports = new ReportsPage(driver)
                .open()
                .setFromDate(today)
                .setToDate(today)
                .runReport();

        Assert.assertTrue(reports.hasResult(), "Should show report result section");
        Assert.assertTrue(reports.visitsPerDayRowCount() >= 1,
                "Visits per day table should have at least 1 row");

        String href = reports.exportCsvHrefOrEmpty();
        Assert.assertTrue(href.contains("ExportCsv"), "Export CSV link should point to Reports/ExportCsv");

        logoutIfPossible();
    }

    @Test
    public void reports_invalidDateRange_showsErrorMessage() {
        logoutIfPossible();
        loginAs("manager");

        String from = TestData.date(LocalDate.now());
        String to = TestData.date(LocalDate.now().minusDays(1)); // invalid (To < From)

        new ReportsPage(driver)
                .open()
                .setFromDate(from)
                .setToDate(to)
                .runReport();

        Assert.assertTrue(driver.getPageSource().contains("Khoảng ngày không hợp lệ"),
                "Should display invalid date range error");

        logoutIfPossible();
    }
}
