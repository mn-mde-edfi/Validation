/*

Error on:
	Course Defined By LEA (District Course) does not refer to Course Defined By SEA (State) or 
	Course Defined By Post-Secondary (College Course) does not refer to Course Defined By LEA (District)

*/

DECLARE @RuleId VARCHAR(32) = '50.00.0001';
DECLARE @Message NVARCHAR(MAX) = 'Course Defined By LEA (District Course) does not refer to Course Defined By SEA (State) or '+
	'Course Defined By Post-Secondary (College Course) does not refer to Course Defined By LEA (District). The entity ID returned is a district ed-org.';
DECLARE @IsError BIT = 1;

WITH 
failed_rows AS (

SELECT 
	Course.CourseCode,
	Course.EducationOrganizationId,
	CourseDefinedByDescriptor.CodeValue CourseDefinedBy,
	ToCourse.CourseCode ToCourseCode,
	ToCourse.EducationOrganizationId ToCourseEducationOrganizationId,
	ToCourseDefinedByDescriptor.CodeValue ToCourseDefinedBy
	
FROM
	mn.CourseCourseAssociation
	INNER JOIN
	edfi.Course
		ON CourseCourseAssociation.CourseCode = Course.CourseCode
		AND CourseCourseAssociation.EducationOrganizationId = Course.EducationOrganizationId
	LEFT JOIN (
		SELECT
			Descriptor.DescriptorId,
			Descriptor.CodeValue
		FROM
			edfi.CourseDefinedByDescriptor
			INNER JOIN
			edfi.Descriptor 
				ON CourseDefinedByDescriptor.CourseDefinedByDescriptorId = Descriptor.DescriptorId
		) CourseDefinedByDescriptor
		ON Course.CourseDefinedByDescriptorId = CourseDefinedByDescriptor.DescriptorId
	INNER JOIN
	edfi.Course ToCourse
		ON CourseCourseAssociation.ToCourseCode = ToCourse.CourseCode
		AND CourseCourseAssociation.ToCourseEducationOrganizationId = ToCourse.EducationOrganizationId
	LEFT JOIN (
		SELECT
			Descriptor.DescriptorId,
			Descriptor.CodeValue
		FROM
			edfi.CourseDefinedByDescriptor
			INNER JOIN
			edfi.Descriptor 
				ON CourseDefinedByDescriptor.CourseDefinedByDescriptorId = Descriptor.DescriptorId
		) ToCourseDefinedByDescriptor
		ON ToCourse.CourseDefinedByDescriptorId = ToCourseDefinedByDescriptor.DescriptorId

WHERE (
	Course.EducationOrganizationId = @DistrictId 
	OR ToCourse.EducationOrganizationId = @DistrictId
	) AND (
	CourseDefinedByDescriptor.CodeValue = 'SEA'
	OR CourseDefinedByDescriptor.CodeValue = 'LEA' AND ToCourseDefinedByDescriptor.CodeValue <> 'SEA'
	OR CourseDefinedByDescriptor.CodeValue = 'College' AND ToCourseDefinedByDescriptor.CodeValue <> 'LEA'
	) 
)
INSERT INTO 
	rules.RuleValidationDetail (RuleValidationId, Id, RuleId, IsError, [Message])
SELECT TOP 1
	@RuleValidationId, @DistrictId, @RuleId RuleId, @IsError IsError, 
	@Message + ' Course code: ' + (SELECT TOP 1 CourseCode+' -> '+ToCourseCode FROM failed_rows) [Message]
FROM failed_rows;
