/*

Error on:

IF SEOA.StudentIndicator.IndicatorName = InternetAccessInResidence and SEOA.StudentIndicator.Indicator <> Yes, 
error on additional record for same student where SEOA.StudentIndicator.IndicatorName = InternetPerformance

*/

DECLARE @RuleId VARCHAR(32) = '60.01.0011';
DECLARE @Message NVARCHAR(MAX) = '';
DECLARE @IsError BIT = 1;

WITH 
failed_rows AS (

SELECT 
FROM
WHERE
  = @DistrictId
)
INSERT INTO 
	rules.RuleValidationDetail (RuleValidationId, Id, RuleId, IsError, [Message])
SELECT TOP 1
	@RuleValidationId, @DistrictId, @RuleId RuleId, @IsError IsError, 
	@Message + CHAR(13)+CHAR(10)+ (
		SELECT TOP 10 
			CourseCode,
			EducationOrganizationId,
			CourseDefinedBy,
			ToCourseCode,
			ToCourseEducationOrganizationId,
			ToCourseDefinedBy
		FROM failed_rows [xml] 
		FOR XML RAW,
			ROOT ('failedRows')
		) [Message]
FROM failed_rows;