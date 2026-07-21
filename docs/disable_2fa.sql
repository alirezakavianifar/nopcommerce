DELETE FROM GenericAttribute WHERE [Key] LIKE '%MultiFactor%' OR [Key] LIKE '%SMS%' OR [Key] LIKE '%TwoFactor%';
UPDATE Setting SET Value='False' WHERE Name LIKE '%MultiFactorAuthenticationSettings.Enabled%';
